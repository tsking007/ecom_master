using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }

    // Human-readable order reference e.g. "ORD-20240115-00034"
    public string OrderNumber { get; set; } = string.Empty;

    // ── Financials ────────────────────────────────────────────────────────────
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal ShippingAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }

    // ── Status ────────────────────────────────────────────────────────────────
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public TrackingStatus TrackingStatus { get; set; } = TrackingStatus.Placed;

    // ── Stripe ────────────────────────────────────────────────────────────────
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    // ── Optional fields ───────────────────────────────────────────────────────
    public string? CouponCode { get; set; }

    // JSON-serialized snapshot of the Address at time of order placement.
    // Stored as a plain JSON string so it survives address deletion/edits.
    public string ShippingAddressSnapshot { get; set; } = string.Empty;

    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? ReturnRequestedAt { get; set; }
    public string? ReturnReason { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}