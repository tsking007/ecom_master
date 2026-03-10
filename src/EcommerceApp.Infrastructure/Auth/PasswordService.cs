using EcommerceApp.Domain.Interfaces;
using System.Text.RegularExpressions;
using BCryptNet = BCrypt.Net.BCrypt;

namespace EcommerceApp.Infrastructure.Auth;

/// <summary>
/// BCrypt password service.
///
/// Work factor 12 produces hashes in ~300-400ms on modern hardware.
/// This is the recommended minimum for new applications in 2024.
/// Signup and password change have ~400ms overhead — acceptable for these
/// low-frequency operations. Login has the same overhead, which is mitigated
/// by the IP-level rate limiter (10 logins per 5 min).
///
/// The 100-character input limit prevents BCrypt's 72-byte input truncation
/// from causing false positives on very long passwords (BCrypt silently
/// truncates inputs at 72 bytes — two passwords that share the same first
/// 72 bytes would both verify correctly).
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    // Compiled regex for performance — PasswordService is singleton
    private static readonly Regex _hasUppercase = new(
        @"[A-Z]",
        RegexOptions.Compiled);

    private static readonly Regex _hasLowercase = new(
        @"[a-z]",
        RegexOptions.Compiled);

    private static readonly Regex _hasDigit = new(
        @"[0-9]",
        RegexOptions.Compiled);

    private static readonly Regex _hasSpecialChar = new(
        @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]",
        RegexOptions.Compiled);

    // ── Hashing ───────────────────────────────────────────────────────────────

    public string HashPassword(string password)
    {
        // BCrypt.Net-Next generates a random 128-bit salt automatically.
        // The salt is embedded in the output hash string — no need to store separately.
        return BCryptNet.HashPassword(password, WorkFactor);
    }

    // ── Verification ──────────────────────────────────────────────────────────

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            // BCrypt.Verify is timing-safe. It takes approximately the same time
            // regardless of how many characters match — prevents timing attacks.
            return BCryptNet.Verify(password, hash);
        }
        catch
        {
            // Protects against malformed hashes in the database
            return false;
        }
    }

    // ── Strength check ────────────────────────────────────────────────────────

    /// <summary>
    /// Server-side strength check used in handler code (not FluentValidation).
    /// The FluentValidation rules in CommonValidators.ApplyPasswordRules() are
    /// the canonical enforcement point — this method is provided as a programmatic
    /// check for cases like password reset where you want to verify strength in code.
    /// </summary>
    public bool MeetsStrengthRequirements(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Length: 8 to 100 characters
        if (password.Length < 8 || password.Length > 100)
            return false;

        // At least one uppercase letter
        if (!_hasUppercase.IsMatch(password))
            return false;

        // At least one lowercase letter
        if (!_hasLowercase.IsMatch(password))
            return false;

        // At least one digit
        if (!_hasDigit.IsMatch(password))
            return false;

        // At least one special character
        if (!_hasSpecialChar.IsMatch(password))
            return false;

        return true;
    }
}