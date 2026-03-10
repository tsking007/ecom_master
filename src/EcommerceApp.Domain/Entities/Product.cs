using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // URL-friendly unique identifier e.g. "apple-iphone-15-pro-128gb"
    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }

    // Total physical stock in warehouse
    public int StockQuantity { get; set; } = 0;

    // Units currently locked by pending Stripe checkout sessions
    public int ReservedQuantity { get; set; } = 0;

    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }

    // Stored as JSON array in the database column
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }
    public List<string> Tags { get; set; } = new();

    public double AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public int SoldCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────

    // The price customers actually pay
    public decimal EffectivePrice => DiscountedPrice ?? Price;

    // % discount shown on the UI (null if no discount)
    public decimal? DiscountPercentage => DiscountedPrice.HasValue && Price > 0
        ? Math.Round((1 - DiscountedPrice.Value / Price) * 100, 0)
        : null;

    // How many units can still be added to cart
    public int AvailableStock => Math.Max(0, StockQuantity - ReservedQuantity);

    // ── Navigation ────────────────────────────────────────────────────────────
    public ICollection<CartItem> CartItems { get; set; }
        = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();
    public ICollection<ProductReview> Reviews { get; set; }
        = new List<ProductReview>();
    public ICollection<Wishlist> WishlistItems { get; set; }
        = new List<Wishlist>();
    public ICollection<StockReservation> StockReservations { get; set; }
        = new List<StockReservation>();
    public ElasticStore? ElasticStoreEntry { get; set; }
}