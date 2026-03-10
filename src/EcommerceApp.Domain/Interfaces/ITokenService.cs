using System.Security.Claims;

namespace EcommerceApp.Domain.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Creates a signed JWT access token.
    /// Expiry: Jwt__AccessTokenExpiryMinutes (default 15 min).
    /// Claims: sub (userId), email, role.
    /// </summary>
    string GenerateAccessToken(Guid userId, string email, string role);

    /// <summary>
    /// Creates a cryptographically random 64-byte Base64Url string.
    /// Stored as a BCrypt hash in TokenStore — never stored raw.
    /// Expiry: Jwt__RefreshTokenExpiryDays (default 7 days).
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Creates a short-lived signed JWT used only for the password reset flow.
    /// Expiry: Jwt__PasswordResetTokenExpiryMinutes (default 10 min).
    /// Claims: sub (userId), email, purpose = "password-reset".
    /// </summary>
    string GeneratePasswordResetToken(Guid userId, string email);

    /// <summary>
    /// Validates the access token signature and expiry.
    /// Returns ClaimsPrincipal on success, null on failure.
    /// </summary>
    ClaimsPrincipal? ValidateAccessToken(string token);

    /// <summary>
    /// Validates the password-reset token signature and expiry.
    /// Returns ClaimsPrincipal on success, null on failure.
    /// </summary>
    ClaimsPrincipal? ValidatePasswordResetToken(string token);

    /// <summary>
    /// Extracts the userId claim from an expired access token without
    /// validating expiry. Used in the silent refresh flow.
    /// Returns null if the token is structurally invalid.
    /// </summary>
    Guid? GetUserIdFromExpiredToken(string token);
}