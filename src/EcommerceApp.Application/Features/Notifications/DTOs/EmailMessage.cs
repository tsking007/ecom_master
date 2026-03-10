namespace EcommerceApp.Application.Features.Notifications.DTOs;

/// <summary>
/// Represents a single outbound email.
/// Built by EmailTemplateEngine and passed to IEmailSender.SendAsync.
/// </summary>
public record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? PlainTextBody = null,
    string? From = null,   // Overrides NotificationSettings.FromEmail if set
    string? FromName = null,
    string? ReplyTo = null);