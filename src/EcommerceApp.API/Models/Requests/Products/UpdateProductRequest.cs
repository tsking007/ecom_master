namespace EcommerceApp.API.Models.Requests.Products;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
}