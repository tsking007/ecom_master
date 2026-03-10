// src/EcommerceApp.Infrastructure/Services/NotificationService.cs
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EcommerceApp.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendOtpAsync(
        string recipient,
        string otp,
        OtpPurpose purpose,
        bool isSms = false,
        CancellationToken cancellationToken = default)
    {
        var channel = isSms ? "SMS" : "EMAIL";
        var purposeLabel = purpose switch
        {
            OtpPurpose.EmailVerification => "Email Verification",
            OtpPurpose.ForgotPassword => "Forgot Password",
            OtpPurpose.PhoneVerification => "Phone Verification",
            _ => purpose.ToString()
        };

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║               DEV OTP — NOT FOR PRODUCTION              ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Channel   : {channel,-44}║");
        Console.WriteLine($"║  Purpose   : {purposeLabel,-44}║");
        Console.WriteLine($"║  Recipient : {recipient,-44}║");
        Console.WriteLine($"║  OTP CODE  : {otp,-44}║");
        Console.WriteLine($"║  Expires   : 10 minutes from now{new string(' ', 27)}║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        _logger.LogInformation(
            "[DEV] OTP | Channel={Channel} | Purpose={Purpose} | " +
            "Recipient={Recipient} | OTP={Otp}",
            channel, purposeLabel, recipient, otp);

        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[DEV] Welcome notification for userId={userId}");
        Console.ResetColor();

        _logger.LogInformation(
            "[DEV] Welcome notification | UserId={UserId}", userId);

        return Task.CompletedTask;
    }
}