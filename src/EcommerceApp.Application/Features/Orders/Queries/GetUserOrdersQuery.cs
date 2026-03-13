using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Orders.DTOs;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Orders.Queries;

public record GetUserOrdersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    TrackingStatus? Status = null
) : IRequest<List<OrderDetailsDto>>;

public class GetUserOrdersQueryHandler
    : IRequestHandler<GetUserOrdersQuery, List<OrderDetailsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUserOrdersQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<OrderDetailsDto>> Handle(
        GetUserOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var pagedResult = await _unitOfWork.Orders.GetByUserIdPagedAsync(
            userId,
            request.PageNumber,
            request.PageSize,
            request.Status,
            cancellationToken);

        return pagedResult.Items
            .Select(order => new OrderDetailsDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                PaymentStatus = order.PaymentStatus.ToString(),
                TrackingStatus = order.TrackingStatus.ToString(),
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                ShippingAmount = order.ShippingAmount,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                ShippingAddressSnapshot = order.ShippingAddressSnapshot,
                CreatedAt = order.CreatedAt,
                Items = order.Items
                    .Select(x => new OrderItemDetailsDto
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        ProductSlug = x.ProductSlug,
                        ProductImageUrl = x.ProductImageUrl,
                        UnitPrice = x.UnitPrice,
                        DiscountedUnitPrice = x.DiscountedUnitPrice,
                        Quantity = x.Quantity,
                        TotalPrice = x.TotalPrice
                    })
                    .ToList()
            })
            .ToList();
    }
}