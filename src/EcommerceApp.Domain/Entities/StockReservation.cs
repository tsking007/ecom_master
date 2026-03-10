using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class StockReservation : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Quantity { get; set; }

    // The Stripe Checkout Session ID that created this reservation
    public string StripeSessionId { get; set; } = string.Empty;

    // Reservation auto-expires after App__StockReservationExpiryMinutes (default 15)
    public DateTime ExpiresAt { get; set; }

    public bool IsReleased { get; set; } = false;
    public DateTime? ReleasedAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}