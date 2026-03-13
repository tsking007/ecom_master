using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Auth.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace EcommerceApp.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenStoreRepository _tokenStoreRepo;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenStoreRepository tokenStoreRepo,
        IPasswordService passwordService,
        ITokenService tokenService,
        IMapper mapper,
        IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _tokenStoreRepo = tokenStoreRepo;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _mapper = mapper;
        _config = config;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Look up user ───────────────────────────────────────────────────
        var user = await _unitOfWork.Users
            .GetByEmailAsync(request.Email, cancellationToken);

        // Return the same generic error for "not found" and "wrong password"
        // to prevent user enumeration attacks.
        const string authError = "Invalid email or password.";

        if (user == null)
            throw new UnauthorizedException(authError);

        // ── 2. Check account status ───────────────────────────────────────────
        if (!user.IsActive)
            throw new UnauthorizedException(
                "Your account has been deactivated. " +
                "Please contact support for assistance.");

        // ── 3. Verify password (BCrypt — timing-safe) ─────────────────────────
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedException(authError);

        // ── 4. Require email verification ─────────────────────────────────────
        if (!user.IsEmailVerified)
            throw new UnauthorizedException(
                "Your email address has not been verified. " +
                "Please check your inbox for the verification OTP.");

        // ── 5. Generate tokens ────────────────────────────────────────────────
        var accessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Email, user.Role.ToString());

        var rawRefreshToken = _tokenService.GenerateRefreshToken();

        var accessExpiryMin = _config.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
        var refreshExpiryDay = _config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

        // ── 6. Persist hashed refresh token ──────────────────────────────────
        // SHA-256 hash allows fast indexed lookup without iterating all sessions.
        var tokenStore = new TokenStore
        {
            UserId = user.Id,
            RefreshToken = TokenHashHelper.HashToken(rawRefreshToken),
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpiryDay),
            IsRevoked = false
        };

        await _tokenStoreRepo.AddAsync(tokenStore, cancellationToken);

        // Both _unitOfWork and _tokenStoreRepo share the same scoped AppDbContext
        // so SaveChangesAsync on either one flushes all staged changes.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 7. Build response ─────────────────────────────────────────────────
        return new AuthResponse(
            User: _mapper.Map<UserDto>(user),
            AccessToken: accessToken,
            RefreshToken: rawRefreshToken,
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(accessExpiryMin),
            RefreshTokenExpiresAt: tokenStore.ExpiresAt);
    }
}