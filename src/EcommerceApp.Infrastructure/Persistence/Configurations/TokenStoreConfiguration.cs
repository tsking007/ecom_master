using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class TokenStoreConfiguration : IEntityTypeConfiguration<TokenStore>
{
    public void Configure(EntityTypeBuilder<TokenStore> builder)
    {
        builder.ToTable("TokenStores");

        builder.HasKey(t => t.Id);

        // ── Properties ────────────────────────────────────────────────────────
        // Base64Url-encoded 64-byte random value = up to 88 chars
        builder.Property(t => t.RefreshToken)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.DeviceInfo)
            .HasMaxLength(500);

        // IPv6 max length is 45 chars
        builder.Property(t => t.IpAddress)
            .HasMaxLength(50);

        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // Session validation — find active tokens for a user
        builder.HasIndex(t => new { t.UserId, t.IsRevoked })
            .HasDatabaseName("IX_TokenStores_UserId_IsRevoked");

        // Cleanup job — find expired tokens
        builder.HasIndex(t => t.ExpiresAt)
            .HasDatabaseName("IX_TokenStores_ExpiresAt");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(t => !t.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(t => t.User)
            .WithMany(u => u.TokenStores)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}