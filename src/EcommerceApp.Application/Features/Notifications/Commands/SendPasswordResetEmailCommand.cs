using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Notifications.Templates;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EcommerceApp.Infrastructure.Notifications;

namespace EcommerceApp.Application.Features.Notifications.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Dispatched by ForgotPasswordCommandHandler.
/// Sends a password reset link containing the signed JWT reset token.
///
/// The frontend URL is constructed here using NotificationSettings.FrontendBaseUrl
/// so handler code never hard-codes URLs.
/// </summary>
public record SendPasswordResetEmailCommand(
    string To,
    string FirstName,
    string ResetToken) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────

public class SendPasswordResetEmailCommandHandler
    : IRequestHandler<SendPasswordResetEmailCommand>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationSettings _settings;
    private readonly ILogger<SendPasswordResetEmailCommandHandler> _logger;

    public SendPasswordResetEmailCommandHandler(
        IEmailSender emailSender,
        IOptions<NotificationSettings> settings,
        ILogger<SendPasswordResetEmailCommandHandler> logger)
    {
        _emailSender = emailSender;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task Handle(
        SendPasswordResetEmailCommand command,
        CancellationToken cancellationToken)
    {
        // Build the full reset URL that the frontend will handle
        var resetUrl =
            $"{_settings.FrontendBaseUrl.TrimEnd('/')}" +
            $"/reset-password?token={Uri.EscapeDataString(command.ResetToken)}";

        var message = EmailTemplateEngine.BuildPasswordResetEmail(
            to: command.To,
            firstName: command.FirstName,
            resetUrl: resetUrl,
            fromEmail: _settings.FromEmail,
            fromName: _settings.FromName);

        var result = await _emailSender.SendAsync(message, cancellationToken);

        if (!result.IsSuccess)
            _logger.LogWarning(
                "Password reset email failed for {Email}: {Error}",
                command.To, result.ErrorMessage);
    }
}