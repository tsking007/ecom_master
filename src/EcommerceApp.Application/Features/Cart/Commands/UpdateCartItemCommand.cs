using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Cart.Commands;

public record UpdateCartItemCommand(Guid CartItemId, int Quantity) : IRequest;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCartItemCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var cart = await _unitOfWork.Carts.GetWithItemsAndProductsAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Cart", userId);

        var item = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId)
            ?? throw new NotFoundException("CartItem", request.CartItemId);

        if (!item.Product.IsActive)
            throw new ValidationException("CartItemId", "Product is inactive.");

        if (request.Quantity > item.Product.AvailableStock)
            throw new ValidationException("Quantity",
                $"Only {item.Product.AvailableStock} units are available.");

        item.Quantity = request.Quantity;
        cart.LastActivityAt = DateTime.UtcNow;

        await _unitOfWork.Carts.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}