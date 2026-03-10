namespace EcommerceApp.Application.Features.Wishlist.DTOs;

public class WishlistDto
{
    public int TotalItems { get; set; }
    public bool HasOutOfStockItems { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
}