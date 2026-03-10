using EcommerceApp.Application.Features.Notifications.DTOs;

namespace EcommerceApp.Application.Common.Interfaces;

/// <summary>
/// Low-level SMS sending abstraction.
/// Implementations: DevSmsSender (Part 10), TwilioSmsSender (Part 12).
/// </summary>
public interface ISmsSender
{
    Task<NotificationResult> SendAsync(
        SmsMessage message,
        CancellationToken cancellationToken = default);
}