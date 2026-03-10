using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenStoreRepository _tokenStoreRepo;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenStoreRepository tokenStoreRepo,
        ITokenService tokenService,
        IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _tokenStoreRepo = tokenStoreRepo;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    public async Task Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Validate the password reset JWT ────────────────────────────────
        var principal = _tokenService.ValidatePasswordResetToken(request.Token);
        if (principal == null)
            throw new ValidationException(
                "Token",
                "This password reset link is invalid or has expired. " +
                "Please request a new one.");

        // ── 2. Extract userId from claims ─────────────────────────────────────
        var userIdClaim = principal.FindFirst("sub")?.Value
                       ?? principal.FindFirst(
                              System.Security.Claims.ClaimTypes.NameIdentifier)
                          ?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new ValidationException("Token", "Invalid token payload.");

        // ── 3. Find user ──────────────────────────────────────────────────────
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), userId);

        if (!user.IsActive)
            throw new UnauthorizedException(
                "Your account has been deactivated.");

        // ── 4. Prevent reuse of the same password ─────────────────────────────
        if (_passwordService.VerifyPassword(request.NewPassword, user.PasswordHash))
            throw new ValidationException(
                "NewPassword",
                "Your new password must be different from your current password.");

        // ── 5. Update password ────────────────────────────────────────────────
        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        // ── 6. Revoke ALL sessions — force re-login on every device ───────────
        await _tokenStoreRepo.RevokeAllByUserIdAsync(userId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}