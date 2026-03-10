using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class OtpStoreConfiguration : IEntityTypeConfiguration<OtpStore>
{
    public void Configure(EntityTypeBuilder<OtpStore> builder)
    {
        builder.ToTable("OtpStores");

        builder.HasKey(o => o.Id);

        // ── Properties ────────────────────────────────────────────────────────
        // Identifier = email address or phone number
        builder.Property(o => o.Identifier)
            .IsRequired()
            .HasMaxLength(256);

        // BCrypt hash of the raw 6-digit OTP
        builder.Property(o => o.OtpHash)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Purpose)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.ExpiresAt).IsRequired();

        builder.Property(o => o.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // Primary lookup during OTP verification
        builder.HasIndex(o => new { o.Identifier, o.Purpose, o.IsUsed })
            .HasDatabaseName("IX_OtpStores_Identifier_Purpose_IsUsed");

        // Cleanup job — find expired OTPs
        builder.HasIndex(o => o.ExpiresAt)
            .HasDatabaseName("IX_OtpStores_ExpiresAt");

        builder.HasIndex(o => o.UserId)
            .HasDatabaseName("IX_OtpStores_UserId");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(o => !o.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        // UserId is nullable — OTPs can be sent before the user account exists
        builder.HasOne(o => o.User)
            .WithMany(u => u.OtpStores)
            .HasForeignKey(o => o.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}