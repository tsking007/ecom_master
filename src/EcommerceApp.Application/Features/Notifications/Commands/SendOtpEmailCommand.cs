using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Notifications.Templates;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EcommerceApp.Infrastructure.Notifications;

namespace EcommerceApp.Application.Features.Notifications.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Dispatched by SignupCommandHandler and VerifyOtpCommandHandler
/// whenever an OTP needs to be sent by email.
///
/// Purpose values: "EmailVerification" | "TwoFactorAuth"
/// </summary>
public record SendOtpEmailCommand(
    string To,
    string FirstName,
    string Otp,
    string Purpose) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────

public class SendOtpEmailCommandHandler : IRequestHandler<SendOtpEmailCommand>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationSettings _settings;
    private readonly ILogger<SendOtpEmailCommandHandler> _logger;

    public SendOtpEmailCommandHandler(
        IEmailSender emailSender,
        IOptions<NotificationSettings> settings,
        ILogger<SendOtpEmailCommandHandler> logger)
    {
        _emailSender = emailSender;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task Handle(
        SendOtpEmailCommand command,
        CancellationToken cancellationToken)
    {
        var message = EmailTemplateEngine.BuildOtpEmail(
            to: command.To,
            firstName: command.FirstName,
            otp: command.Otp,
            purpose: command.Purpose,
            fromEmail: _settings.FromEmail,
            fromName: _settings.FromName);

        var result = await _emailSender.SendAsync(message, cancellationToken);

        if (!result.IsSuccess)
            _logger.LogWarning(
                "OTP email failed for {Email} purpose={Purpose}: {Error}",
                command.To, command.Purpose, result.ErrorMessage);
    }
}