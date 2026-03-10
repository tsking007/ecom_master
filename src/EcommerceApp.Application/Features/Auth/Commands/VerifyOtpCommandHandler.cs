using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Auth.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace EcommerceApp.Application.Features.Auth.Commands;

public class VerifyOtpCommandHandler
    : IRequestHandler<VerifyOtpCommand, VerifyOtpResponse>
{
    private const int MaxOtpAttempts = 5;
    private const int OtpExpiryMinutes = 10;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpStoreRepository _otpStoreRepo;
    private readonly ITokenStoreRepository _tokenStoreRepo;
    private readonly IOtpService _otpService;
    private readonly ITokenService _tokenService;
    private readonly INotificationService _notificationService;
    private readonly IRateLimitService _rateLimitService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public VerifyOtpCommandHandler(
        IUnitOfWork unitOfWork,
        IOtpStoreRepository otpStoreRepo,
        ITokenStoreRepository tokenStoreRepo,
        IOtpService otpService,
        ITokenService tokenService,
        INotificationService notificationService,
        IRateLimitService rateLimitService,
        IMapper mapper,
        IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _otpStoreRepo = otpStoreRepo;
        _tokenStoreRepo = tokenStoreRepo;
        _otpService = otpService;
        _tokenService = tokenService;
        _notificationService = notificationService;
        _rateLimitService = rateLimitService;
        _mapper = mapper;
        _config = config;
    }

    public async Task<VerifyOtpResponse> Handle(
        VerifyOtpCommand request,
        CancellationToken cancellationToken)
    {
        var identifier = request.Identifier.ToLowerInvariant().Trim();

        // ── 1. Rate limit check ────────────────────────────────────────────────
        if (!await _rateLimitService.IsOtpVerifyAllowedAsync(
            identifier, cancellationToken))
        {
            var remaining = await _rateLimitService.GetRemainingBlockDurationAsync(
                identifier, "OTP_VERIFY", cancellationToken);

            throw new ValidationException(
                "Otp",
                $"Too many failed attempts. " +
                $"Please try again in {remaining?.Minutes ?? 30} minutes.");
        }

        // ── 2. Find the latest valid OTP record ───────────────────────────────
        var otpRecord = await _otpStoreRepo.GetLatestUnusedAsync(
            identifier, request.Purpose, cancellationToken);

        if (otpRecord == null)
            throw new ValidationException(
                "Otp",
                "No valid OTP found. " +
                "Please request a new one.");

        // ── 3. Check attempt count ────────────────────────────────────────────
        if (otpRecord.AttemptCount >= MaxOtpAttempts)
        {
            otpRecord.IsUsed = true;    // invalidate exhausted OTP
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ValidationException(
                "Otp",
                "This OTP has expired due to too many incorrect attempts. " +
                "Please request a new one.");
        }

        // ── 4. Verify OTP (BCrypt — timing-safe) ─────────────────────────────
        if (!_otpService.VerifyOtp(request.Otp, otpRecord.OtpHash))
        {
            otpRecord.AttemptCount++;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _rateLimitService.RecordOtpVerifyFailAsync(
                identifier, cancellationToken);

            var remaining = MaxOtpAttempts - otpRecord.AttemptCount;
            throw new ValidationException(
                "Otp",
                $"Incorrect OTP. " +
                $"You have {remaining} attempt(s) remaining.");
        }

        // ── 5. Mark OTP as used ───────────────────────────────────────────────
        otpRecord.IsUsed = true;
        otpRecord.UpdatedAt = DateTime.UtcNow;

        await _rateLimitService.RecordOtpVerifySuccessAsync(
            identifier, cancellationToken);

        // ── 6. Handle each purpose ────────────────────────────────────────────
        return request.Purpose switch
        {
            OtpPurpose.EmailVerification =>
                await HandleEmailVerificationAsync(
                    otpRecord, request, cancellationToken),

            OtpPurpose.ForgotPassword =>
                await HandleForgotPasswordAsync(
                    identifier, cancellationToken),

            OtpPurpose.PhoneVerification =>
                await HandlePhoneVerificationAsync(
                    otpRecord, cancellationToken),

            _ =>
                await HandleGenericOtpAsync(cancellationToken)
        };
    }

    // ── Email verification — auto-login after confirming ──────────────────────

    private async Task<VerifyOtpResponse> HandleEmailVerificationAsync(
        OtpStore otpRecord,
        VerifyOtpCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users
            .GetByEmailAsync(otpRecord.Identifier, cancellationToken)
            ?? throw new NotFoundException(
                "User", otpRecord.Identifier);

        user.IsEmailVerified = true;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send welcome email (fire-and-forget failure is OK — non-critical)
        try
        {
            await _notificationService.SendWelcomeAsync(
                user.Id, cancellationToken);
        }
        catch { /* log in Part 23 middleware, don't fail login */ }

        // Issue session tokens
        var accessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Email, user.Role.ToString());
        var rawRefreshToken = _tokenService.GenerateRefreshToken();

        var refreshExpiryDays = _config.GetValue<int>(
            "Jwt:RefreshTokenExpiryDays", 7);
        var accessExpiryMin = _config.GetValue<int>(
            "Jwt:AccessTokenExpiryMinutes", 15);

        var tokenStore = new TokenStore
        {
            UserId = user.Id,
            RefreshToken = TokenHashHelper.HashToken(rawRefreshToken),
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpiryDays),
            IsRevoked = false
        };

        await _tokenStoreRepo.AddAsync(tokenStore, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var auth = new AuthResponse(
            User: _mapper.Map<UserDto>(user),
            AccessToken: accessToken,
            RefreshToken: rawRefreshToken,
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(accessExpiryMin),
            RefreshTokenExpiresAt: tokenStore.ExpiresAt);

        return new VerifyOtpResponse(
            IsAuthenticated: true,
            Auth: auth,
            PasswordResetToken: null,
            Message: "Email verified successfully. Welcome aboard!");
    }

    // ── Forgot password — return a short-lived JWT for the reset form ─────────

    private async Task<VerifyOtpResponse> HandleForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users
            .GetByEmailAsync(email, cancellationToken)
            ?? throw new NotFoundException("User", email);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resetToken = _tokenService.GeneratePasswordResetToken(
            user.Id, user.Email);

        return new VerifyOtpResponse(
            IsAuthenticated: false,
            Auth: null,
            PasswordResetToken: resetToken,
            Message: "OTP verified. " +
                     "Use the token to set your new password.");
    }

    // ── Phone verification ────────────────────────────────────────────────────

    private async Task<VerifyOtpResponse> HandlePhoneVerificationAsync(
        OtpStore otpRecord,
        CancellationToken cancellationToken)
    {
        if (otpRecord.UserId.HasValue)
        {
            var user = await _unitOfWork.Users
                .GetByIdAsync(otpRecord.UserId.Value, cancellationToken);

            if (user != null)
            {
                user.IsPhoneVerified = true;
                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VerifyOtpResponse(
            IsAuthenticated: false,
            Auth: null,
            PasswordResetToken: null,
            Message: "Phone number verified successfully.");
    }

    // ── Generic fallback for ChangeEmail, ChangePhone, TwoFactorAuth ──────────
    // Full implementations are in Part 21 (User profile commands).

    private async Task<VerifyOtpResponse> HandleGenericOtpAsync(
        CancellationToken cancellationToken)
    {
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VerifyOtpResponse(
            IsAuthenticated: false,
            Auth: null,
            PasswordResetToken: null,
            Message: "OTP verified successfully.");
    }
}