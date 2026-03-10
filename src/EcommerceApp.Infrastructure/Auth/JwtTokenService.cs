using EcommerceApp.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceApp.Infrastructure.Auth;

/// <summary>
/// JWT implementation of ITokenService.
/// Uses HMAC-SHA256 signing. All tokens are stateless JWTs — validation
/// does not require a database round-trip.
///
/// Token types:
///   Access token       — 15 min, carries userId/email/role claims
///   Refresh token      — 7 days, 64-byte cryptographically random string
///                        (NOT a JWT — stored hashed in TokenStore)
///   Password-reset JWT — 10 min, contains "purpose":"password-reset" claim
///
/// Thread safety: this service is registered as Singleton. It is safe because
/// all fields are readonly and initialized once in the constructor.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _signingCredentials;

    // Pre-built validation params — avoids allocating per request
    private readonly TokenValidationParameters _standardParams;
    private readonly TokenValidationParameters _expiredParams;

    private readonly JwtSecurityTokenHandler _handler;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.Secret))
            throw new InvalidOperationException(
                "JWT Secret is not configured. " +
                "Set Jwt:Secret in appsettings or environment variables.");

        if (_settings.Secret.Length < 32)
            throw new InvalidOperationException(
                "JWT Secret must be at least 32 characters long " +
                "to ensure sufficient signing entropy.");

        _signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Secret));

        _signingCredentials = new SigningCredentials(
            _signingKey, SecurityAlgorithms.HmacSha256);

        _handler = new JwtSecurityTokenHandler();

        // ── Standard validation (validates lifetime) ──────────────────────────
        _standardParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,
            // 30-second clock skew accommodates minor time drift between servers
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // ── Expired token params (skips lifetime check — for silent refresh) ──
        _expiredParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = false   // ← intentionally skip expiry
        };
    }

    // ── Access token ──────────────────────────────────────────────────────────

    public string GenerateAccessToken(Guid userId, string email, string role)
    {
        var claims = new[]
        {
            // sub: the user's unique identifier
            new Claim(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            // email: used by frontend and middleware
            new Claim(JwtRegisteredClaimNames.Email, email),
            // role: used by [Authorize(Roles = "Admin")] on controllers
            new Claim(ClaimTypes.Role,               role),
            // jti: unique token ID — used for token blacklisting if needed
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString("N")),
            // iat: issued-at in Unix epoch seconds
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow
                                    .AddMinutes(_settings.AccessTokenExpiryMinutes),
            signingCredentials: _signingCredentials);

        return _handler.WriteToken(token);
    }

    // ── Refresh token ─────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a 64-byte cryptographically random string.
    ///
    /// WHY NOT a JWT:
    ///   Refresh tokens are stored hashed in the database. Making them JWTs would
    ///   create a signed token that remains structurally valid even after the
    ///   database session is revoked — a security anti-pattern.
    ///   A plain random string stored hashed is simpler and more secure.
    ///
    /// 64 bytes = 512 bits of entropy. Brute-force is computationally infeasible.
    /// </summary>
    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    // ── Password reset token ──────────────────────────────────────────────────

    public string GeneratePasswordResetToken(Guid userId, string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            // purpose claim: validated in ValidatePasswordResetToken
            // to prevent an access token from being used as a reset token
            new Claim("purpose", "password-reset"),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow
                                    .AddMinutes(
                                        _settings.PasswordResetTokenExpiryMinutes),
            signingCredentials: _signingCredentials);

        return _handler.WriteToken(token);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var principal = _handler.ValidateToken(
                token, _standardParams, out _);
            return principal;
        }
        catch
        {
            // Token is invalid, expired, or structurally malformed
            return null;
        }
    }

    public ClaimsPrincipal? ValidatePasswordResetToken(string token)
    {
        try
        {
            var principal = _handler.ValidateToken(
                token, _standardParams, out _);

            // Extra guard: confirm this token was issued specifically for password reset.
            // An attacker who intercepts a valid access token cannot use it here.
            var purpose = principal.FindFirst("purpose")?.Value;
            if (purpose != "password-reset")
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public Guid? GetUserIdFromExpiredToken(string token)
    {
        try
        {
            // ValidateLifetime = false: we deliberately accept expired access tokens
            // so that the refresh flow can identify the user whose token has expired.
            var principal = _handler.ValidateToken(
                token, _expiredParams, out _);

            // Try both sub and NameIdentifier — different middleware sets different claims
            var userIdStr =
                principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdStr, out var userId)
                ? userId
                : null;
        }
        catch
        {
            return null;
        }
    }
}