namespace EcommerceApp.Domain.Interfaces;

public interface IOtpService
{
    /// <summary>
    /// Generates a cryptographically random 6-digit numeric OTP string.
    /// Uses RNGCryptoServiceProvider — never use Random.
    /// </summary>
    string GenerateOtp();

    /// <summary>BCrypt-hashes the raw OTP for storage in OtpStore.</summary>
    string HashOtp(string otp);

    /// <summary>
    /// Compares a raw OTP against a stored BCrypt hash using
    /// a timing-safe comparison to prevent timing attacks.
    /// </summary>
    bool VerifyOtp(string otp, string hash);
}