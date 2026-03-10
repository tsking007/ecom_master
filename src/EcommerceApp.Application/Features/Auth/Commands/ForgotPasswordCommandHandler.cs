using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand>
{
    private const int OtpExpiryMinutes = 10;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpStoreRepository _otpStoreRepo;
    private readonly IOtpService _otpService;
    private readonly INotificationService _notificationService;
    private readonly IRateLimitService _rateLimitService;

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpStoreRepository otpStoreRepo,
        IOtpService otpService,
        INotificationService notificationService,
        IRateLimitService rateLimitService)
    {
        _unitOfWork = unitOfWork;
        _otpStoreRepo = otpStoreRepo;
        _otpService = otpService;
        _notificationService = notificationService;
        _rateLimitService = rateLimitService;
    }

    public async Task Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant().Trim();

        // ── 1. Rate limit check ────────────────────────────────────────────────
        if (!await _rateLimitService.IsOtpSendAllowedAsync(
            email, cancellationToken))
        {
            // Silently return — don't reveal whether email exists or is rate-limited
            return;
        }

        // ── 2. Look up user ───────────────────────────────────────────────────
        var user = await _unitOfWork.Users
            .GetByEmailAsync(email, cancellationToken);

        // SECURITY: return silently whether the email exists or not.
        // This prevents user enumeration (attackers can't check if email is registered).
        if (user == null || !user.IsActive || user.IsDeleted)
            return;

        // ── 3. Invalidate previous OTPs & generate new one ────────────────────
        await _otpStoreRepo.InvalidateAllPreviousAsync(
            email, OtpPurpose.ForgotPassword, cancellationToken);

        var rawOtp = _otpService.GenerateOtp();
        var otpHash = _otpService.HashOtp(rawOtp);

        var otpStore = new OtpStore
        {
            UserId = user.Id,
            Identifier = email,
            OtpHash = otpHash,
            Purpose = OtpPurpose.ForgotPassword,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed = false,
            AttemptCount = 0
        };

        await _otpStoreRepo.AddAsync(otpStore, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 4. Record rate limit & send OTP ──────────────────────────────────
        await _rateLimitService.RecordOtpSendAsync(email, cancellationToken);

        await _notificationService.SendOtpAsync(
            recipient: email,
            otp: rawOtp,
            purpose: OtpPurpose.ForgotPassword,
            isSms: false,
            cancellationToken: cancellationToken);
    }
}