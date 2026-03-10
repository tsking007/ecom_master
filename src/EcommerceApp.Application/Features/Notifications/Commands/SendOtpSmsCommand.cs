using EcommerceApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EcommerceApp.Infrastructure.Notifications;

namespace EcommerceApp.Application.Features.Notifications.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Dispatched when an OTP needs to be delivered by SMS.
/// PhoneNumber must be in E.164 format: +12345678900
/// Purpose values: "PhoneVerification" | "TwoFactorAuth"
/// </summary>
public record SendOtpSmsCommand(
    string PhoneNumber,
    string Otp,
    string Purpose) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────

public class SendOtpSmsCommandHandler : IRequestHandler<SendOtpSmsCommand>
{
    private readonly ISmsSender _smsSender;
    private readonly NotificationSettings _settings;
    private readonly ILogger<SendOtpSmsCommandHandler> _logger;

    public SendOtpSmsCommandHandler(
        ISmsSender smsSender,
        IOptions<NotificationSettings> settings,
        ILogger<SendOtpSmsCommandHandler> logger)
    {
        _smsSender = smsSender;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task Handle(
        SendOtpSmsCommand command,
        CancellationToken cancellationToken)
    {
        var purposeText = command.Purpose switch
        {
            "PhoneVerification" => "verify your phone number",
            "TwoFactorAuth" => "complete your login",
            _ => "complete your request"
        };

        var body =
            $"Your EcommerceApp code is: {command.Otp}\n" +
            $"Use this to {purposeText}. Expires in 10 minutes.\n" +
            $"Do not share this code with anyone.";

        var message = new DTOs.SmsMessage(
            To: command.PhoneNumber,
            Body: body);

        var result = await _smsSender.SendAsync(message, cancellationToken);

        if (!result.IsSuccess)
            _logger.LogWarning(
                "OTP SMS failed for {Phone} purpose={Purpose}: {Error}",
                command.PhoneNumber, command.Purpose, result.ErrorMessage);
    }
}