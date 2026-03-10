using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class RateLimitLog : BaseEntity
{
    // IP address or email/phone used as the rate-limit key
    public string Identifier { get; set; } = string.Empty;

    // "OTP_SEND" | "OTP_VERIFY" | "LOGIN"
    public string Action { get; set; } = string.Empty;

    // Total attempts within the current window
    public int AttemptCount { get; set; } = 0;

    // Failed verification attempts (used to trigger account block)
    public int FailedAttemptCount { get; set; } = 0;

    // Set when the identifier is temporarily blocked
    public DateTime? BlockedUntil { get; set; }

    // When the current rate-limit window started
    public DateTime WindowStartedAt { get; set; } = DateTime.UtcNow;

    // Last time this identifier made an attempt
    public DateTime LastAttemptAt { get; set; } = DateTime.UtcNow;

    // ── Computed ──────────────────────────────────────────────────────────────
    public bool IsCurrentlyBlocked
        => BlockedUntil.HasValue && BlockedUntil.Value > DateTime.UtcNow;
}