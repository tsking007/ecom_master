using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Wishlist.Commands;

public record AddToWishlistCommand(Guid ProductId) : IRequest;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddToWishlistCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(
        AddToWishlistCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var product = await _unitOfWork.Products.GetByIdAsync(
            request.ProductId,
            cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (!product.IsActive)
            throw new ValidationException(
                "ProductId",
                "Inactive products cannot be added to wishlist.");

        var existing = await _unitOfWork.Wishlists.GetByUserAndProductAsync(
            userId,
            request.ProductId,
            cancellationToken);

        if (existing != null)
            return; // idempotent behavior

        var wishlistItem = new EcommerceApp.Domain.Entities.Wishlist
        {
            UserId = userId,
            ProductId = product.Id,
            PriceAtAdd = product.EffectivePrice
        };

        await _unitOfWork.Wishlists.AddAsync(wishlistItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}