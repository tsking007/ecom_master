using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");

        builder.HasKey(s => s.Id);

        // ── Properties ────────────────────────────────────────────────────────
        builder.Property(s => s.Quantity).IsRequired();

        // Stripe checkout session that created this reservation
        builder.Property(s => s.StripeSessionId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ExpiresAt).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // Cleanup job — find expired, unreleased reservations
        builder.HasIndex(s => new { s.ExpiresAt, s.IsReleased })
            .HasDatabaseName("IX_StockReservations_ExpiresAt_IsReleased");

        // Webhook handler — look up reservation by Stripe session
        builder.HasIndex(s => s.StripeSessionId)
            .HasDatabaseName("IX_StockReservations_StripeSessionId");

        builder.HasIndex(s => new { s.ProductId, s.IsReleased })
            .HasDatabaseName("IX_StockReservations_ProductId_IsReleased");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(s => !s.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(s => s.User)
            .WithMany(u => u.StockReservations)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        // Product FK is configured in ProductConfiguration
    }
}