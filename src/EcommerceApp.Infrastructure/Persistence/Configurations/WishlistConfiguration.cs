using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");

        builder.HasKey(w => w.Id);

        // ── Properties ────────────────────────────────────────────────────────
        // Effective price when the item was wishlisted — used for price-drop detection
        builder.Property(w => w.PriceAtAdd)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // One wishlist entry per user per product
        //builder.HasIndex(w => new { w.UserId, w.ProductId })
        //    .IsUnique()
        //    .HasDatabaseName("IX_Wishlists_UserId_ProductId");
        builder.HasIndex(w => new { w.UserId, w.ProductId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Wishlists_UserId_ProductId");

        // PriceDropAlertService — find items eligible for price-drop notification
        builder.HasIndex(w => new { w.ProductId, w.PriceDropAlertSentAt })
            .HasDatabaseName("IX_Wishlists_ProductId_PriceDropAlertSentAt");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(w => !w.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(w => w.User)
            .WithMany(u => u.WishlistItems)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        // Product FK is configured in ProductConfiguration
    }
}