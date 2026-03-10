using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Notifications.Templates;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EcommerceApp.Infrastructure.Notifications;


namespace EcommerceApp.Application.Features.Notifications.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Dispatched by VerifyOtpCommandHandler after a successful EmailVerification OTP.
/// </summary>
public record SendWelcomeEmailCommand(
    string To,
    string FirstName) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────

public class SendWelcomeEmailCommandHandler : IRequestHandler<SendWelcomeEmailCommand>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationSettings _settings;
    private readonly ILogger<SendWelcomeEmailCommandHandler> _logger;

    public SendWelcomeEmailCommandHandler(
        IEmailSender emailSender,
        IOptions<NotificationSettings> settings,
        ILogger<SendWelcomeEmailCommandHandler> logger)
    {
        _emailSender = emailSender;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task Handle(
        SendWelcomeEmailCommand command,
        CancellationToken cancellationToken)
    {
        var message = EmailTemplateEngine.BuildWelcomeEmail(
            to: command.To,
            firstName: command.FirstName,
            fromEmail: _settings.FromEmail,
            fromName: _settings.FromName);

        var result = await _emailSender.SendAsync(message, cancellationToken);

        if (!result.IsSuccess)
            _logger.LogWarning(
                "Welcome email failed for {Email}: {Error}",
                command.To, result.ErrorMessage);
    }
}