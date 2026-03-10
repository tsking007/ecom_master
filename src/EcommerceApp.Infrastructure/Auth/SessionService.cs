using EcommerceApp.Application.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace EcommerceApp.Infrastructure.Auth;

/// <summary>
/// Higher-level session management service built on top of ITokenStoreRepository.
///
/// Responsibilities:
///   - Create new sessions (called after login and OTP verification)
///   - Validate existing sessions (called by SessionValidationMiddleware)
///   - Rotate sessions (called by RefreshTokenCommandHandler)
///   - Revoke sessions (called by logout and password reset)
///   - Capture device fingerprint for session audit log
///
/// This service is scoped (per-request) because it wraps a scoped
/// ITokenStoreRepository and IUnitOfWork.
/// </summary>
public class SessionService
{
    private readonly ITokenStoreRepository _tokenStoreRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public SessionService(
        ITokenStoreRepository tokenStoreRepo,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions)
    {
        _tokenStoreRepo = tokenStoreRepo;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
    }

    // ── Session creation ──────────────────────────────────────────────────────

    /// <summary>
    /// Stores a hashed refresh token as a new session record.
    /// Called by LoginCommandHandler and VerifyOtpCommandHandler
    /// after successful authentication.
    /// </summary>
    public async Task<TokenStore> CreateSessionAsync(
        Guid userId,
        string rawRefreshToken,
        string? deviceInfo,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var tokenStore = new TokenStore
        {
            UserId = userId,
            RefreshToken = TokenHashHelper.HashToken(rawRefreshToken),
            DeviceInfo = SanitizeDeviceInfo(deviceInfo),
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow
                               .AddDays(_jwtSettings.RefreshTokenExpiryDays),
            IsRevoked = false
        };

        await _tokenStoreRepo.AddAsync(tokenStore, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tokenStore;
    }

    // ── Session validation ────────────────────────────────────────────────────

    /// <summary>
    /// Retrieves a valid (non-revoked, non-expired) session for the given user
    /// and raw refresh token. Returns null if no valid session exists.
    ///
    /// Called by SessionValidationMiddleware on every authenticated request
    /// to confirm the session is still active.
    /// </summary>
    public async Task<TokenStore?> GetValidSessionAsync(
        Guid userId,
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var hashedToken = TokenHashHelper.HashToken(rawRefreshToken);

        return await _tokenStoreRepo
            .GetByHashedTokenAsync(hashedToken, userId, cancellationToken);
    }

    // ── Session rotation ──────────────────────────────────────────────────────

    /// <summary>
    /// Implements refresh token rotation:
    ///   1. Marks the old session as revoked
    ///   2. Creates a new session with fresh tokens
    ///
    /// Rotation means each refresh token can only be used once.
    /// If a stolen refresh token is used, the legitimate user's
    /// next request will fail (the token was already rotated),
    /// alerting them to the compromise.
    /// </summary>
    public async Task<TokenStore> RotateSessionAsync(
        TokenStore oldSession,
        string newRawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        // Revoke the old session
        oldSession.IsRevoked = true;
        oldSession.UpdatedAt = DateTime.UtcNow;

        // Create new session inheriting device info from old session
        var newSession = new TokenStore
        {
            UserId = oldSession.UserId,
            RefreshToken = TokenHashHelper.HashToken(newRawRefreshToken),
            DeviceInfo = oldSession.DeviceInfo,
            IpAddress = oldSession.IpAddress,
            ExpiresAt = DateTime.UtcNow
                                  .AddDays(_jwtSettings.RefreshTokenExpiryDays),
            IsRevoked = false,
            LastRefreshedAt = DateTime.UtcNow
        };

        await _tokenStoreRepo.AddAsync(newSession, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newSession;
    }

    // ── Session revocation ────────────────────────────────────────────────────

    /// <summary>
    /// Revokes a single session. Called by LogoutCommandHandler.
    /// Silently succeeds if the session does not exist (idempotent).
    /// </summary>
    public async Task RevokeSessionAsync(
        Guid userId,
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var hashedToken = TokenHashHelper.HashToken(rawRefreshToken);

        var session = await _tokenStoreRepo
            .GetByHashedTokenAsync(hashedToken, userId, cancellationToken);

        if (session == null) return;

        session.IsRevoked = true;
        session.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Revokes all sessions for a user.
    /// Called by ResetPasswordCommandHandler and ChangePasswordCommandHandler
    /// to force re-login on every device after a credential change.
    /// </summary>
    public async Task RevokeAllSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _tokenStoreRepo.RevokeAllByUserIdAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Heartbeat ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates LastRefreshedAt on an active session.
    /// Called by SessionValidationMiddleware after a successful silent refresh
    /// so the admin session audit log shows activity.
    /// </summary>
    public async Task TouchSessionAsync(
        TokenStore session,
        CancellationToken cancellationToken = default)
    {
        session.LastRefreshedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Active session list ───────────────────────────────────────────────────

    /// <summary>
    /// Returns all non-revoked sessions for a user.
    /// Used on a "Manage Devices" page to show the user where they are logged in.
    /// </summary>
    public async Task<IReadOnlyList<TokenStore>> GetActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _tokenStoreRepo
            .GetActiveTokensByUserIdAsync(userId, cancellationToken);
    }

    // ── Device info ───────────────────────────────────────────────────────────

    /// <summary>
    /// Truncates and sanitizes the User-Agent string before storage.
    /// Full User-Agent strings can be up to 1000+ characters —
    /// we cap at 500 to match the column definition.
    /// </summary>
    private static string? SanitizeDeviceInfo(string? deviceInfo)
    {
        if (string.IsNullOrWhiteSpace(deviceInfo))
            return null;

        // Cap at 500 chars to match the DeviceInfo column max length
        return deviceInfo.Length > 500
            ? deviceInfo[..500]
            : deviceInfo;
    }
}