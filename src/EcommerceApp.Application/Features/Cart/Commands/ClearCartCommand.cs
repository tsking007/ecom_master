using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Cart.Commands;

public record ClearCartCommand : IRequest;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ClearCartCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken);

        if (cart == null)
            return;

        await _unitOfWork.Carts.ClearCartAsync(cart.Id, cancellationToken);

        cart.LastActivityAt = DateTime.UtcNow;
        await _unitOfWork.Carts.UpdateAsync(cart, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}