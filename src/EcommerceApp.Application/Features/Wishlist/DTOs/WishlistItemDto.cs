namespace EcommerceApp.Application.Features.Wishlist.DTOs;

public class WishlistItemDto
{
    public Guid WishlistId { get; set; }
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public string? Brand { get; set; }

    public decimal PriceAtAdd { get; set; }
    public decimal CurrentPrice { get; set; }
    public bool HasPriceChanged { get; set; }
    public bool HasPriceDropped { get; set; }

    public int AvailableStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool IsActiveProduct { get; set; }

    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }

    public DateTime AddedAt { get; set; }
}