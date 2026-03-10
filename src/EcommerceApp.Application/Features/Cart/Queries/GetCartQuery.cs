using EcommerceApp.Application.Common;
using EcommerceApp.Application.Features.Cart.DTOs;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Cart.Queries;

public record GetCartQuery : IRequest<CartDto>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCartQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var cart = await _unitOfWork.Carts.GetWithItemsAndProductsAsync(userId, cancellationToken);

        if (cart == null)
        {
            return new CartDto
            {
                CartId = Guid.Empty,
                UserId = userId,
                TotalItems = 0,
                Subtotal = 0,
                HasOutOfStockItems = false,
                HasPriceChanges = false,
                Items = new List<CartItemDto>()
            };
        }

        var items = cart.Items.Select(i =>
        {
            var currentPrice = i.Product.EffectivePrice;
            var availableStock = i.Product.AvailableStock;
            var isOutOfStock = !i.Product.IsActive || availableStock <= 0 || i.Quantity > availableStock;
            var hasPriceChanged = i.UnitPrice != currentPrice;

            return new CartItemDto
            {
                CartItemId = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ProductSlug = i.Product.Slug,
                MainImageUrl = i.Product.ImageUrls.FirstOrDefault(),
                Quantity = i.Quantity,
                PriceAtAddition = i.UnitPrice,
                CurrentUnitPrice = currentPrice,
                LineTotal = currentPrice * i.Quantity,
                AvailableStock = availableStock,
                IsOutOfStock = isOutOfStock,
                HasPriceChanged = hasPriceChanged,
                IsActiveProduct = i.Product.IsActive
            };
        }).ToList();

        return new CartDto
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            TotalItems = items.Sum(x => x.Quantity),
            Subtotal = items.Sum(x => x.LineTotal),
            HasOutOfStockItems = items.Any(x => x.IsOutOfStock),
            HasPriceChanges = items.Any(x => x.HasPriceChanged),
            Items = items
        };
    }
}