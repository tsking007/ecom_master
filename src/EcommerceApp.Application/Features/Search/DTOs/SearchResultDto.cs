namespace EcommerceApp.Application.Features.Search.DTOs;

/// <summary>
/// Search result item returned from either Elasticsearch or the SQL fallback.
/// Shape is identical regardless of which backend is active so the frontend
/// never needs to change when the search provider is switched.
/// </summary>
public class SearchResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public decimal EffectivePrice { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? MainImageUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    public bool IsActive { get; set; }
    /// <summary>
    /// Relevance score from Elasticsearch, or a computed relevance value
    /// from the SQL fallback. Used for result ranking.
    /// </summary>
    public double? Score { get; set; }
}