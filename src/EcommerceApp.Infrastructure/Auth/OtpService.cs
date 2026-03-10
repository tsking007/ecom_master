using EcommerceApp.Domain.Interfaces;
using System.Security.Cryptography;
using BCryptNet = BCrypt.Net.BCrypt;

namespace EcommerceApp.Infrastructure.Auth;

/// <summary>
/// BCrypt-backed OTP service.
///
/// ── Why BCrypt for OTPs? ────────────────────────────────────────────────────
/// OTPs are 6 decimal digits — only 1,000,000 possible values. If an attacker
/// ever reads the OtpStore table, they could brute-force every possible OTP
/// in milliseconds with a plain hash (MD5/SHA). BCrypt's intentional slowness
/// (work factor 10 ≈ 100ms per verify) makes offline brute-force impractical.
/// Combined with attempt-count limits and rate limiting, the system is secure.
///
/// ── Why RandomNumberGenerator for generation? ───────────────────────────────
/// System.Random is not cryptographically secure — its output can be predicted
/// given enough samples. RandomNumberGenerator.GetInt32 uses the OS entropy
/// source (CryptGenRandom on Windows, /dev/urandom on Linux) and applies
/// internal rejection sampling to guarantee uniform distribution.
/// </summary>
public class OtpService : IOtpService
{
    // Work factor 10: ~100ms on modern hardware.
    // High enough to slow brute-force, low enough for UX (OTP verify feels instant).
    // Do NOT increase to 12+ here — BCrypt at WF 12 takes 300-400ms per verify,
    // which would add visible latency to every OTP endpoint call.
    private const int BcryptWorkFactor = 10;

    // ── OTP generation ────────────────────────────────────────────────────────

    public string GenerateOtp()
    {
        // RandomNumberGenerator.GetInt32(fromInclusive, toExclusive)
        // Available since .NET 6. Uses internal rejection sampling → no modulo bias.
        // Range: [0, 1_000_000) → gives exactly 6 digits (000000 to 999999).
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);

        // D6 zero-pads: 42 → "000042", 999999 → "999999"
        return value.ToString("D6");
    }

    // ── Hashing ───────────────────────────────────────────────────────────────

    public string HashOtp(string otp)
    {
        // EnhancedEntropy = false: BCrypt.Net-Next's "enhanced entropy" mode
        // pre-hashes the input with SHA384, changing the algorithm's behavior.
        // We keep it off so the raw 6-digit string is hashed directly —
        // the BCrypt output is still 60 chars and just as secure for 6-digit inputs.
        return BCryptNet.HashPassword(otp, BcryptWorkFactor, enhancedEntropy: false);
    }

    // ── Verification ──────────────────────────────────────────────────────────

    public bool VerifyOtp(string otp, string hash)
    {
        try
        {
            // BCrypt.Verify is timing-safe by design — it always takes ~workFactor time
            // regardless of whether the guess is right or wrong. This prevents
            // timing-based side-channel attacks.
            return BCryptNet.Verify(otp, hash, enhancedEntropy: false);
        }
        catch
        {
            // Verify throws FormatException if the hash string is malformed.
            // Treat a malformed hash as a failed verification.
            return false;
        }
    }
}