using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    // Nullable — the product record may be soft-deleted after the order was placed.
    // The snapshot fields below preserve all the data we need regardless.
    public Guid? ProductId { get; set; }

    // ── Snapshots (captured at checkout, immutable after that) ────────────────
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string? ProductSlug { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountedUnitPrice { get; set; }

    // ── Quantities & totals ───────────────────────────────────────────────────
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public Order Order { get; set; } = null!;
    public Product? Product { get; set; }
}