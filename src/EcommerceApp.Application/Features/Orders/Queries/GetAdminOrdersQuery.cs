using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Orders.DTOs;
using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Orders.Queries;

/// <summary>
/// Admin-only paginated order list.
/// Supports filtering by TrackingStatus, PaymentStatus, date range,
/// and free-text search on order number / customer name / email.
/// </summary>
public record GetAdminOrdersQuery(
    int PageNumber,
    int PageSize,
    TrackingStatus? TrackingStatus,
    PaymentStatus? PaymentStatus,
    DateTime? From,
    DateTime? To,
    string? SearchTerm
) : IRequest<PagedResult<AdminOrderSummaryDto>>;

public class GetAdminOrdersQueryHandler
    : IRequestHandler<GetAdminOrdersQuery, PagedResult<AdminOrderSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AdminOrderSummaryDto>> Handle(
        GetAdminOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var pagedOrders = await _unitOfWork.Orders.GetAdminPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            trackingStatus: request.TrackingStatus,
            paymentStatus: request.PaymentStatus,
            from: request.From,
            to: request.To,
            searchTerm: request.SearchTerm,
            cancellationToken: cancellationToken);

        return pagedOrders.Map(order => new AdminOrderSummaryDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerEmail = order.User?.Email ?? string.Empty,
            CustomerName = order.User != null
                                  ? $"{order.User.FirstName} {order.User.LastName}".Trim()
                                  : string.Empty,
            PaymentStatus = order.PaymentStatus.ToString(),
            TrackingStatus = order.TrackingStatus.ToString(),
            TotalAmount = order.TotalAmount,
            ItemCount = order.Items.Count,
            CreatedAt = order.CreatedAt
        });
    }
}