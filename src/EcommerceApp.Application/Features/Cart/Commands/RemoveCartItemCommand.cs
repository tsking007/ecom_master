using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Cart.Commands;

public record RemoveCartItemCommand(Guid CartItemId) : IRequest;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveCartItemCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var cart = await _unitOfWork.Carts.GetWithItemsAndProductsAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Cart", userId);

        var item = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId)
            ?? throw new NotFoundException("CartItem", request.CartItemId);

        await _unitOfWork.Carts.RemoveCartItemAsync(item, cancellationToken);

        cart.LastActivityAt = DateTime.UtcNow;
        await _unitOfWork.Carts.UpdateAsync(cart, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}