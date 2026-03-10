using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    // Price snapshot taken when the item was added.
    // Used to detect if the live price has changed since adding to cart.
    public decimal UnitPrice { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}