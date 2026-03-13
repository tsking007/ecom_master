using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(256);

        // Composite unique index: same key cannot be reused by the same user
        // but two different users can coincidentally send the same string key
        builder.HasIndex(x => new { x.IdempotencyKey, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_IdempotencyRecords_Key_User");

        // For fast expiry cleanup queries
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("IX_IdempotencyRecords_ExpiresAt");

        builder.Property(x => x.ResponseJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.StripeSessionId)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}