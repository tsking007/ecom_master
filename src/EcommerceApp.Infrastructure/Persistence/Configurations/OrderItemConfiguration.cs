using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        // ── Properties ────────────────────────────────────────────────────────
        // ProductId is nullable — product may be soft-deleted after the order.
        // The snapshot fields below preserve everything needed for display.

        // Snapshot fields — immutable after order is placed
        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(oi => oi.ProductImageUrl)
            .HasMaxLength(500);

        builder.Property(oi => oi.ProductSlug)
            .HasMaxLength(300);

        builder.Property(oi => oi.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(oi => oi.DiscountedUnitPrice)
            .HasColumnType("decimal(18, 2)");

        builder.Property(oi => oi.Quantity).IsRequired();

        builder.Property(oi => oi.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(oi => oi.CreatedAt).IsRequired();
        builder.Property(oi => oi.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(oi => oi.OrderId)
            .HasDatabaseName("IX_OrderItems_OrderId");

        builder.HasIndex(oi => oi.ProductId)
            .HasDatabaseName("IX_OrderItems_ProductId");

        // ── Soft-delete query filter ──────────────────────────────────────────
        //builder.HasQueryFilter(oi => !oi.IsDeleted);
        // Order FK and Product FK are configured in their respective configurations
    }
}