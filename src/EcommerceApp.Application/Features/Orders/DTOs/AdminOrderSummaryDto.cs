namespace EcommerceApp.Application.Features.Orders.DTOs;

/// <summary>
/// Lightweight order row returned by the admin order list endpoint.
/// Does NOT include full item details — use GetOrderDetailsQuery for that.
/// </summary>
public class AdminOrderSummaryDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    // Customer snapshot — safe even if the user record is later deleted
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;
    public string TrackingStatus { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    // Convenience count — avoids loading full items on the list page
    public int ItemCount { get; set; }

    public DateTime CreatedAt { get; set; }
}