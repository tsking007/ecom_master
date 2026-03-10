namespace EcommerceApp.Domain.Interfaces;

public interface IEmailService
{
    /// <summary>Convenience overload for simple single-recipient emails.</summary>
    Task<bool> SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default);

    /// <summary>Full-control overload used by the NotificationService.</summary>
    Task<bool> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a fully composed outbound email.
/// Produced by the template engine and passed to the active IEmailService provider.
/// </summary>
public record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null,
    string? FromEmail = null,
    string? FromName = null
);