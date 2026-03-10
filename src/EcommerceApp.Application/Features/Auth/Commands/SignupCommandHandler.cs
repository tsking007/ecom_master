using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Auth.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public class SignupCommandHandler : IRequestHandler<SignupCommand, SignupResponse>
{
    private const int OtpExpiryMinutes = 10;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpStoreRepository _otpStoreRepo;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly INotificationService _notificationService;
    private readonly IRateLimitService _rateLimitService;

    public SignupCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpStoreRepository otpStoreRepo,
        IPasswordService passwordService,
        IOtpService otpService,
        INotificationService notificationService,
        IRateLimitService rateLimitService)
    {
        _unitOfWork = unitOfWork;
        _otpStoreRepo = otpStoreRepo;
        _passwordService = passwordService;
        _otpService = otpService;
        _notificationService = notificationService;
        _rateLimitService = rateLimitService;
    }

    public async Task<SignupResponse> Handle(
        SignupCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant().Trim();

        // ── 1. Rate limit check (OTP send limit) ──────────────────────────────
        if (!await _rateLimitService.IsOtpSendAllowedAsync(email, cancellationToken))
            throw new ValidationException(
                "Email",
                "Too many signup attempts. " +
                "Please wait 15 minutes before trying again.");

        // ── 2. Uniqueness checks ──────────────────────────────────────────────
        if (await _unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException(
                "An account with this email address already exists.");

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
            await _unitOfWork.Users.PhoneExistsAsync(
                request.PhoneNumber, cancellationToken))
            throw new ConflictException(
                "An account with this phone number already exists.");

        // ── 3. Create user (unverified) ───────────────────────────────────────
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = UserRole.Customer,
            IsEmailVerified = false,
            IsPhoneVerified = false,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        // ── 4. Generate OTP & invalidate any previous ones ────────────────────
        await _otpStoreRepo.InvalidateAllPreviousAsync(
            email, OtpPurpose.EmailVerification, cancellationToken);

        var rawOtp = _otpService.GenerateOtp();
        var otpHash = _otpService.HashOtp(rawOtp);

        var otpStore = new OtpStore
        {
            UserId = user.Id,
            Identifier = email,
            OtpHash = otpHash,
            Purpose = OtpPurpose.EmailVerification,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed = false,
            AttemptCount = 0
        };

        await _otpStoreRepo.AddAsync(otpStore, cancellationToken);

        // ── 5. Persist (user + OTP in one save) ──────────────────────────────
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 6. Record OTP send for rate limiting ──────────────────────────────
        await _rateLimitService.RecordOtpSendAsync(email, cancellationToken);

        // ── 7. Send verification OTP email ────────────────────────────────────
        // Awaited here so the caller knows if the email failed.
        // The notification retry system (Part 12) handles transient failures.
        await _notificationService.SendOtpAsync(
            recipient: email,
            otp: rawOtp,
            purpose: OtpPurpose.EmailVerification,
            isSms: false,
            cancellationToken: cancellationToken);

        return new SignupResponse(
            Email: email,
            Message: $"A 6-digit verification code has been sent to {email}. " +
                     $"It expires in {OtpExpiryMinutes} minutes.");
    }
}