namespace EcommerceApp.Application.Features.Orders.DTOs;

public class OrderDetailsDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string TrackingStatus { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? ShippingAddressSnapshot { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDetailsDto> Items { get; set; } = new();
}

public class OrderItemDetailsDto
{
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountedUnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}