using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class Cart : BaseEntity
{
    // One cart per user — enforced via unique index in EF config
    public Guid UserId { get; set; }

    // Updated on every add / update / remove action.
    // Used by the CartReminderService to find idle carts.
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ────────────────────────────────────────────────────────────
    public User User { get; set; } = null!;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}