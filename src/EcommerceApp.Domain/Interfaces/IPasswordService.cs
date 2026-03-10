namespace EcommerceApp.Domain.Interfaces;

public interface IPasswordService
{
    /// <summary>Returns a BCrypt hash of the plain-text password (work factor 12).</summary>
    string HashPassword(string password);

    /// <summary>Verifies a plain-text password against a BCrypt hash.</summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Returns true if the password meets the minimum strength policy:
    /// at least 8 characters, at least one uppercase, one digit, one special character.
    /// Application-layer validators use this for a consistent rule.
    /// </summary>
    bool MeetsStrengthRequirements(string password);
}