namespace EcommerceApp.Application.Features.Cart.DTOs;

public class CartDto
{
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public int TotalItems { get; set; }
    public decimal Subtotal { get; set; }
    public bool HasOutOfStockItems { get; set; }
    public bool HasPriceChanges { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}