using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Wishlist.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Wishlist.Queries;

public record GetWishlistQuery : IRequest<WishlistDto>;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, WishlistDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetWishlistQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<WishlistDto> Handle(
        GetWishlistQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var wishlistItems = await _unitOfWork.Wishlists
            .GetByUserIdAsync(userId, cancellationToken);

        var items = wishlistItems.Select(w =>
        {
            var currentPrice = w.Product.EffectivePrice;
            var availableStock = w.Product.AvailableStock;
            var isOutOfStock = !w.Product.IsActive || availableStock <= 0;

            return new WishlistItemDto
            {
                WishlistId = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
                ProductSlug = w.Product.Slug,
                MainImageUrl = w.Product.ImageUrls.FirstOrDefault(),
                Brand = w.Product.Brand,
                PriceAtAdd = w.PriceAtAdd,
                CurrentPrice = currentPrice,
                HasPriceChanged = currentPrice != w.PriceAtAdd,
                HasPriceDropped = currentPrice < w.PriceAtAdd,
                AvailableStock = availableStock,
                IsOutOfStock = isOutOfStock,
                IsActiveProduct = w.Product.IsActive,
                Category = w.Product.Category,
                SubCategory = w.Product.SubCategory,
                AddedAt = w.CreatedAt
            };
        }).ToList();

        return new WishlistDto
        {
            TotalItems = items.Count,
            HasOutOfStockItems = items.Any(i => i.IsOutOfStock),
            Items = items
        };
    }
}