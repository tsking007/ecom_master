using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class RateLimitLogConfiguration : IEntityTypeConfiguration<RateLimitLog>
{
    public void Configure(EntityTypeBuilder<RateLimitLog> builder)
    {
        builder.ToTable("RateLimitLogs");

        builder.HasKey(r => r.Id);

        // ── Properties ────────────────────────────────────────────────────────
        // IP address, email, or phone number used as the rate-limit key
        builder.Property(r => r.Identifier)
            .IsRequired()
            .HasMaxLength(256);

        // "OTP_SEND" | "OTP_VERIFY" | "LOGIN"
        builder.Property(r => r.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.FailedAttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.WindowStartedAt).IsRequired();
        builder.Property(r => r.LastAttemptAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        // Computed C# property — not mapped to a column
        builder.Ignore(r => r.IsCurrentlyBlocked);

        // ── Indexes ───────────────────────────────────────────────────────────
        // Primary lookup — must be fast as it runs on every OTP request
        builder.HasIndex(r => new { r.Identifier, r.Action })
            .IsUnique()
            .HasDatabaseName("IX_RateLimitLogs_Identifier_Action");

        // Quickly find blocked identifiers
        builder.HasIndex(r => r.BlockedUntil)
            .HasDatabaseName("IX_RateLimitLogs_BlockedUntil");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}