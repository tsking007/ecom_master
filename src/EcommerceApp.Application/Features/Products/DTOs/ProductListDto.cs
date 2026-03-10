namespace EcommerceApp.Application.Features.Products.DTOs;

/// <summary>
/// Lightweight product card — returned by GetProductsQuery and GetBestsellersQuery.
/// Omits full description, all images, tags, and dimensions to keep payload small.
/// </summary>
public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public decimal EffectivePrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int AvailableStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? MainImageUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
}