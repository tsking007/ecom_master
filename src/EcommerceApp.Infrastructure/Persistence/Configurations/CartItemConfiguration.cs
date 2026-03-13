using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        // ── Properties ────────────────────────────────────────────────────────
        builder.Property(ci => ci.Quantity)
            .IsRequired();

        // Price snapshot taken when the item was added to cart
        builder.Property(ci => ci.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(ci => ci.CreatedAt).IsRequired();
        builder.Property(ci => ci.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // A cart cannot have the same product twice — merge quantities instead
        builder.HasIndex(ci => new { ci.CartId, ci.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_CartId_ProductId");

        builder.HasIndex(ci => ci.CartId)
            .HasDatabaseName("IX_CartItems_CartId");

        // ── Soft-delete query filter ──────────────────────────────────────────
        //builder.HasQueryFilter(ci => !ci.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
        // Product FK is configured in ProductConfiguration
    }
}