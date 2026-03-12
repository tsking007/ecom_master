using EcommerceApp.API.Models.Requests.Orders;
using EcommerceApp.Application.Features.Orders.DTOs;
using EcommerceApp.Application.Features.Orders.Queries;
using EcommerceApp.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.API.Controllers.Admin;

/// <summary>
/// Admin order management.
/// Route: /api/v1/admin/orders
/// All endpoints require the "Admin" role.
/// </summary>
[ApiController]
[Route("api/v1/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public AdminOrdersController(ISender sender) => _sender = sender;

    // -------------------------------------------------------------------------
    // GET /api/v1/admin/orders
    // -------------------------------------------------------------------------
    /// <summary>
    /// Paginated list of all orders.
    /// Optional filters: trackingStatus, paymentStatus, from, to, searchTerm.
    ///
    /// Examples:
    ///   GET /api/v1/admin/orders?trackingStatus=Shipped&amp;pageSize=50
    ///   GET /api/v1/admin/orders?paymentStatus=Paid&amp;from=2024-01-01&amp;to=2024-01-31
    ///   GET /api/v1/admin/orders?searchTerm=ORD-20240115
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminOrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] GetAdminOrdersRequest req,
        CancellationToken ct)
    {
        var query = new GetAdminOrdersQuery(
            PageNumber: req.PageNumber,
            PageSize: req.PageSize,
            TrackingStatus: req.TrackingStatus,
            PaymentStatus: req.PaymentStatus,
            From: req.From,
            To: req.To,
            SearchTerm: req.SearchTerm);

        return Ok(await _sender.Send(query, ct));
    }
}