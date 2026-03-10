namespace EcommerceApp.Application.Features.Products.DTOs;

/// <summary>
/// Full product detail — returned by GetProductBySlugQuery.
/// Used on the product detail page.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public decimal EffectivePrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string? MainImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}