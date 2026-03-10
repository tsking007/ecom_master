namespace EcommerceApp.Application.Features.Cart.DTOs;

public class CartItemDto
{
    public Guid CartItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }

    public int Quantity { get; set; }

    // Snapshot at time of adding
    public decimal PriceAtAddition { get; set; }

    // Live price from Product
    public decimal CurrentUnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public int AvailableStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool HasPriceChanged { get; set; }
    public bool IsActiveProduct { get; set; }
}