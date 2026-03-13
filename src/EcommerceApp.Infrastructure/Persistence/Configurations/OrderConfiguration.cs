using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        // ── Properties ────────────────────────────────────────────────────────
        // e.g. "ORD-20240115-00034"
        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.SubTotal)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(o => o.DiscountAmount)
            .IsRequired()
            .HasColumnType("decimal(18, 2)")
            .HasDefaultValue(0m);

        builder.Property(o => o.ShippingAmount)
            .IsRequired()
            .HasColumnType("decimal(18, 2)")
            .HasDefaultValue(0m);

        builder.Property(o => o.TaxAmount)
            .IsRequired()
            .HasColumnType("decimal(18, 2)")
            .HasDefaultValue(0m);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(o => o.PaymentStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.TrackingStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.StripeSessionId)
            .HasMaxLength(200);

        builder.Property(o => o.StripePaymentIntentId)
            .HasMaxLength(200);

        builder.Property(o => o.CouponCode)
            .HasMaxLength(50);

        // JSON snapshot of the address at the time of order placement.
        // Stored as a plain string — never a foreign key.
        builder.Property(o => o.ShippingAddressSnapshot)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(o => o.Notes)
            .HasMaxLength(500);

        builder.Property(o => o.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        builder.Property(o => o.ReturnReason)
            .HasMaxLength(500);

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_Orders_OrderNumber");

        // Customer order history page
        builder.HasIndex(o => new { o.UserId, o.CreatedAt })
            .HasDatabaseName("IX_Orders_UserId_CreatedAt");

        // Stripe webhook lookup
        builder.HasIndex(o => o.StripeSessionId)
            .HasDatabaseName("IX_Orders_StripeSessionId");

        // Admin dashboard filters
        builder.HasIndex(o => new { o.TrackingStatus, o.PaymentStatus })
            .HasDatabaseName("IX_Orders_TrackingStatus_PaymentStatus");

        builder.HasIndex(o => o.CreatedAt)
            .HasDatabaseName("IX_Orders_CreatedAt");

        // ── Soft-delete query filter ──────────────────────────────────────────
        //builder.HasQueryFilter(o => !o.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}