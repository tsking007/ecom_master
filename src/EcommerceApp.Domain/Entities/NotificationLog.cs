using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Entities;

public class NotificationLog : BaseEntity
{
    // Nullable — system-level notifications may not belong to a specific user
    public Guid? UserId { get; set; }

    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    // Email subject or SMS sender label
    public string? Subject { get; set; }

    // Rendered template body (HTML for email, plain text for SMS)
    public string Body { get; set; } = string.Empty;

    // Email address or phone number the message was sent to
    public string Recipient { get; set; } = string.Empty;

    // e.g. "otp-email", "order-confirmation-sms"
    public string? TemplateName { get; set; }

    public string? ErrorMessage { get; set; }

    // Incremented on each automatic retry attempt
    public int RetryCount { get; set; } = 0;

    public DateTime? SentAt { get; set; }
    public DateTime? NextRetryAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public User? User { get; set; }
}