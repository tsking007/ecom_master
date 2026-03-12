using EcommerceApp.Domain.Enums;

namespace EcommerceApp.API.Models.Requests.Orders;

/// <summary>
/// Bound from the query string via [FromQuery] on GET /api/v1/admin/orders.
/// Every field is optional — omitting a field means "no filter on that field".
/// </summary>
public class GetAdminOrdersRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // Filter by tracking status e.g. ?trackingStatus=Shipped
    public TrackingStatus? TrackingStatus { get; set; }

    // Filter by payment status e.g. ?paymentStatus=Paid
    public PaymentStatus? PaymentStatus { get; set; }

    // Date range filters — inclusive on both ends
    // e.g. ?from=2024-01-01&to=2024-01-31
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    // Free-text search on order number, customer email, or customer name
    public string? SearchTerm { get; set; }
}