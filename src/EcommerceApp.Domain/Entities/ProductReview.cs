using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }

    // 1 to 5 — enforced by FluentValidation in the application layer
    public int Rating { get; set; }

    public string? Title { get; set; }
    public string Comment { get; set; } = string.Empty;

    // True when the reviewer has a completed order containing this product
    public bool IsVerifiedPurchase { get; set; } = false;

    // Reviews are hidden until an admin approves them
    public bool IsApproved { get; set; } = false;

    // Up-vote count (future feature)
    public int HelpfulCount { get; set; } = 0;

    // ── Navigation ────────────────────────────────────────────────────────────
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}