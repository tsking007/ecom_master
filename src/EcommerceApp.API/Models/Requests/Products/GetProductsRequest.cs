namespace EcommerceApp.API.Models.Requests.Products;

/// <summary>
/// Bound from URL query string via [FromQuery].
/// IsActive is used by AdminProductsController only.
/// The public ProductsController always overrides it to true.
/// </summary>
public class GetProductsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? MinRating { get; set; }
    public string? Brand { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public bool? IsActive { get; set; }   // admin only — public ignores this
}