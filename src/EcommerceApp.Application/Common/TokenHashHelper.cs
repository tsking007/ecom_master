using System.Security.Cryptography;
using System.Text;

namespace EcommerceApp.Application.Common;

/// <summary>
/// Hashes refresh tokens using SHA-256 before storing them in the database.
///
/// WHY SHA-256 (not BCrypt) for refresh tokens:
///   - Refresh tokens are 64 cryptographically random bytes (256-bit entropy).
///     They cannot be brute-forced regardless of hashing algorithm.
///   - SHA-256 is deterministic → enables indexed DB lookup in O(1).
///   - BCrypt is intentionally slow → looking up a session on every
///     authenticated request would add 100-400ms of latency per request.
///
/// WHY BCrypt for OTPs:
///   - OTPs are 6 digits (only 1,000,000 possible values → brute-forceable).
///   - BCrypt's slowness is necessary to make offline brute-force attacks
///     computationally infeasible even if the OtpStore table is compromised.
/// </summary>
public static class TokenHashHelper
{
    public static string HashToken(string rawToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}