using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Entities;

public class OtpStore : BaseEntity
{
    // UserId is nullable — OTP can be sent before the user account is confirmed
    public Guid? UserId { get; set; }

    // The email address or phone number the OTP was sent to
    public string Identifier { get; set; } = string.Empty;

    // BCrypt hash of the raw 6-digit OTP — never store the raw value
    public string OtpHash { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;

    // Counts failed verification attempts for this OTP record
    public int AttemptCount { get; set; } = 0;

    // ── Navigation ────────────────────────────────────────────────────────────
    public User? User { get; set; }
}