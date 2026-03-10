using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class ElasticStore : BaseEntity
{
    // 1-to-1 with Product — one record per product in SQL mirror table
    public Guid ProductId { get; set; }

    // Denormalized fields copied from Product for fast SQL full-text search
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }
    public List<string> Tags { get; set; } = new();
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public double AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public int SoldCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? ThumbnailImageUrl { get; set; }

    // When this row was last synced from the Products table
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ────────────────────────────────────────────────────────────
    public Product Product { get; set; } = null!;
}