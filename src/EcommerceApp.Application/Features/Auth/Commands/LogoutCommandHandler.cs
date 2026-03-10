using EcommerceApp.Application.Common;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenStoreRepository _tokenStoreRepo;

    public LogoutCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenStoreRepository tokenStoreRepo)
    {
        _unitOfWork = unitOfWork;
        _tokenStoreRepo = tokenStoreRepo;
    }

    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        // Hash the raw refresh token for the indexed DB lookup
        var hashedToken = TokenHashHelper.HashToken(request.RefreshToken);

        var tokenStore = await _tokenStoreRepo
            .GetByHashedTokenAsync(
                hashedToken, request.UserId, cancellationToken);

        // Silently succeed if token not found — user is already logged out
        if (tokenStore == null) return;

        tokenStore.IsRevoked = true;
        tokenStore.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}