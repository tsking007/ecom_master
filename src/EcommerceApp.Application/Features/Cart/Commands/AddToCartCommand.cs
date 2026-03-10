using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Cart.Commands;

public record AddToCartCommand(Guid ProductId, int Quantity) : IRequest;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddToCartCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (!product.IsActive)
            throw new ValidationException("ProductId", "Product is inactive and cannot be added to cart.");

        if (product.AvailableStock < request.Quantity)
            throw new ValidationException("Quantity",
                $"Only {product.AvailableStock} units are available.");

        var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken);

        if (cart == null)
        {
            cart = new Domain.Entities.Cart
            {
                UserId = userId,
                LastActivityAt = DateTime.UtcNow
            };

            await _unitOfWork.Carts.AddAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var existingItem = await _unitOfWork.Carts.GetCartItemAsync(cart.Id, request.ProductId, cancellationToken);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + request.Quantity;

            if (newQuantity > product.AvailableStock)
                throw new ValidationException("Quantity",
                    $"Cannot add more than available stock. Available: {product.AvailableStock}.");

            existingItem.Quantity = newQuantity;
            cart.LastActivityAt = DateTime.UtcNow;

            await _unitOfWork.Carts.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var item = new CartItem
        {
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = request.Quantity,
            UnitPrice = product.EffectivePrice
        };

        await _unitOfWork.Carts.AddCartItemAsync(item, cancellationToken);

        cart.LastActivityAt = DateTime.UtcNow;
        await _unitOfWork.Carts.UpdateAsync(cart, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}