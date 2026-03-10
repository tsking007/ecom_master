using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Manages refresh token sessions.
/// Injected directly into Auth handlers — not exposed via IUnitOfWork.
/// Refresh tokens are stored as SHA-256 hashes (not BCrypt) to allow
/// fast indexed lookups without iterating all sessions per user.
/// </summary>
public interface ITokenStoreRepository : IRepository<TokenStore>
{
    /// <summary>
    /// Looks up an active (non-revoked, non-expired) token by
    /// its SHA-256 hash and the owning user's ID.
    /// The userId check prevents one user from using another user's token.
    /// </summary>
    Task<TokenStore?> GetByHashedTokenAsync(
        string hashedRefreshToken,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all non-revoked, non-expired sessions for a user.
    /// Used by the admin "active sessions" feature (future).
    /// </summary>
    Task<IReadOnlyList<TokenStore>> GetActiveTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks every session for a user as revoked.
    /// Called after a password reset to force re-authentication
    /// on all devices.
    /// </summary>
    Task RevokeAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}