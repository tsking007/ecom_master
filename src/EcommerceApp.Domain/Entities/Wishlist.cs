using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }

    // Effective price when the item was wishlisted.
    // Used to detect a price drop and trigger the price-drop-alert notification.
    public decimal PriceAtAdd { get; set; }

    // Set after we send a price-drop alert to prevent duplicate alerts
    public DateTime? PriceDropAlertSentAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}