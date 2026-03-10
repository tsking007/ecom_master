using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Auth.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace EcommerceApp.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenStoreRepository _tokenStoreRepo;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenStoreRepository tokenStoreRepo,
        ITokenService tokenService,
        IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _tokenStoreRepo = tokenStoreRepo;
        _tokenService = tokenService;
        _config = config;
    }

    public async Task<TokenResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Extract userId from the (possibly expired) access token ─────────
        var userId = _tokenService.GetUserIdFromExpiredToken(request.AccessToken)
            ?? throw new UnauthorizedException(
                "Invalid access token. Please log in again.");

        // ── 2. Look up the session by hashed refresh token ────────────────────
        var hashedToken = TokenHashHelper.HashToken(request.RefreshToken);

        var tokenStore = await _tokenStoreRepo
            .GetByHashedTokenAsync(hashedToken, userId, cancellationToken)
            ?? throw new UnauthorizedException(
                "Invalid or expired refresh token. Please log in again.");

        // ── 3. Load user to generate new access token with fresh claims ────────
        var user = await _unitOfWork.Users
            .GetByIdAsync(tokenStore.UserId, cancellationToken)
            ?? throw new UnauthorizedException(
                "User account not found. Please log in again.");

        if (!user.IsActive)
            throw new UnauthorizedException(
                "Your account has been deactivated.");

        // ── 4. Rotate: revoke old token, issue new ones ───────────────────────
        tokenStore.IsRevoked = true;
        tokenStore.UpdatedAt = DateTime.UtcNow;

        var newAccessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Email, user.Role.ToString());

        var newRawRefreshToken = _tokenService.GenerateRefreshToken();

        var refreshExpiryDays = _config.GetValue<int>(
            "Jwt:RefreshTokenExpiryDays", 7);
        var accessExpiryMin = _config.GetValue<int>(
            "Jwt:AccessTokenExpiryMinutes", 15);

        var newTokenStore = new TokenStore
        {
            UserId = user.Id,
            RefreshToken = TokenHashHelper.HashToken(newRawRefreshToken),
            DeviceInfo = tokenStore.DeviceInfo,
            IpAddress = tokenStore.IpAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpiryDays),
            IsRevoked = false,
            LastRefreshedAt = DateTime.UtcNow
        };

        await _tokenStoreRepo.AddAsync(newTokenStore, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRawRefreshToken,
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(accessExpiryMin),
            RefreshTokenExpiresAt: newTokenStore.ExpiresAt);
    }
}