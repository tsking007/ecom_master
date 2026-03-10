using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Notifications.DTOs;
using Microsoft.Extensions.Logging;

namespace EcommerceApp.Infrastructure.Notifications;

/// <summary>
/// Development-only ISmsSender — logs SMS details to the console.
/// Replaced by TwilioSmsSender in Part 12.
/// </summary>
public class DevSmsSender : ISmsSender
{
    private readonly ILogger<DevSmsSender> _logger;

    public DevSmsSender(ILogger<DevSmsSender> logger)
    {
        _logger = logger;
    }

    public Task<NotificationResult> SendAsync(
        SmsMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[DEV SMS] ─────────────────────────────────────────\n" +
            "  To  : {To}\n" +
            "  Body: {Body}\n" +
            "───────────────────────────────────────────────────",
            message.To,
            message.Body);

        var messageId = $"dev-sms-{Guid.NewGuid():N}";
        return Task.FromResult(NotificationResult.Success(messageId));
    }
}