using EcommerceApp.Application.Features.Notifications.DTOs;

namespace EcommerceApp.Application.Common.Interfaces;

/// <summary>
/// Low-level email sending abstraction.
/// Implementations: DevEmailSender (Part 10), SendGridEmailSender (Part 11).
/// Registered as Transient — provider clients are typically thread-safe.
/// </summary>
public interface IEmailSender
{
    Task<NotificationResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);
}