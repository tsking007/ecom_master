using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Wishlist.Commands;

public record MoveWishlistItemToCartCommand(Guid WishlistId) : IRequest;

public class MoveWishlistItemToCartCommandHandler
    : IRequestHandler<MoveWishlistItemToCartCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public MoveWishlistItemToCartCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(
        MoveWishlistItemToCartCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var wishlistItem = await _unitOfWork.Wishlists.GetByIdAsync(
            request.WishlistId,
            cancellationToken)
            ?? throw new NotFoundException("WishlistItem", request.WishlistId);

        if (wishlistItem.UserId != userId)
            throw new ForbiddenException(
                "You do not have permission to move this wishlist item.");

        var product = await _unitOfWork.Products.GetByIdAsync(
            wishlistItem.ProductId,
            cancellationToken)
            ?? throw new NotFoundException("Product", wishlistItem.ProductId);

        if (!product.IsActive)
            throw new ValidationException(
                "WishlistId",
                "Product is inactive and cannot be moved to cart.");

        if (product.AvailableStock < 1)
            throw new ValidationException(
                "WishlistId",
                "Product is out of stock.");

        var cart = await _unitOfWork.Carts.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (cart == null)
        {
            cart = new EcommerceApp.Domain.Entities.Cart
            {
                UserId = userId,
                LastActivityAt = DateTime.UtcNow
            };

            await _unitOfWork.Carts.AddAsync(cart, cancellationToken);
        }

        var existingCartItem = await _unitOfWork.Carts.GetCartItemAsync(
            cart.Id,
            product.Id,
            cancellationToken);

        if (existingCartItem != null)
        {
            var newQuantity = existingCartItem.Quantity + 1;

            if (newQuantity > product.AvailableStock)
                throw new ValidationException(
                    "WishlistId",
                    $"Cannot move item to cart because only {product.AvailableStock} units are available.");

            existingCartItem.Quantity = newQuantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = 1,
                UnitPrice = product.EffectivePrice
            };

            await _unitOfWork.Carts.AddCartItemAsync(
                cartItem,
                cancellationToken);
        }

        cart.LastActivityAt = DateTime.UtcNow;
        await _unitOfWork.Carts.UpdateAsync(cart, cancellationToken);

        //await _unitOfWork.Wishlists.SoftDeleteAsync(
        //    wishlistItem,
        //    cancellationToken);

        await _unitOfWork.Wishlists.HardDeleteAsync(
            wishlistItem,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}