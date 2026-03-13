using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        // ── Properties ────────────────────────────────────────────────────────
        builder.Property(c => c.LastActivityAt).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // One cart per user — enforced
        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Carts_UserId");

        // CartReminderService — find idle carts older than N days
        builder.HasIndex(c => c.LastActivityAt)
            .HasDatabaseName("IX_Carts_LastActivityAt");

        // ── Soft-delete query filter ──────────────────────────────────────────
        //builder.HasQueryFilter(c => !c.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}