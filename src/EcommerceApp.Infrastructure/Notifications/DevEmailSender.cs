using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Notifications.DTOs;
using Microsoft.Extensions.Logging;

namespace EcommerceApp.Infrastructure.Notifications;

/// <summary>
/// Development-only IEmailSender — logs email details to the console
/// instead of sending a real email.
///
/// Replaced by SendGridEmailSender in Part 11.
/// Only registered when ASPNETCORE_ENVIRONMENT = Development (see DI wiring).
///
/// To see OTPs during local development:
///   Check the console / Debug Output window in Visual Studio.
///   The OTP code will appear in the log entry.
/// </summary>
public class DevEmailSender : IEmailSender
{
    private readonly ILogger<DevEmailSender> _logger;

    public DevEmailSender(ILogger<DevEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<NotificationResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[DEV EMAIL] ──────────────────────────────────────\n" +
            "  To      : {To}\n" +
            "  Subject : {Subject}\n" +
            "  Body    :\n{Body}\n" +
            "──────────────────────────────────────────────────",
            message.To,
            message.Subject,
            StripHtml(message.HtmlBody));

        var messageId = $"dev-{Guid.NewGuid():N}";
        return Task.FromResult(NotificationResult.Success(messageId));
    }

    /// <summary>
    /// Strips HTML tags for readable console output.
    /// Not production-grade — just enough for dev logs.
    /// </summary>
    private static string StripHtml(string html)
    {
        // Remove tags
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        // Collapse whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s{2,}", "\n");
        return text.Trim();
    }
}