using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");

        builder.HasKey(n => n.Id);

        // ── Properties ────────────────────────────────────────────────────────
        builder.Property(n => n.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<int>();

        // Email subject line or SMS sender label
        builder.Property(n => n.Subject)
            .HasMaxLength(500);

        // Rendered HTML or plain-text body — can be very large
        builder.Property(n => n.Body)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        // Email address or phone number
        builder.Property(n => n.Recipient)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(n => n.TemplateName)
            .HasMaxLength(100);

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(n => n.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // Notification bell — unread in-app notifications for a user
        builder.HasIndex(n => new { n.UserId, n.Status })
            .HasDatabaseName("IX_NotificationLogs_UserId_Status");

        // Admin log viewer filters
        builder.HasIndex(n => new { n.Channel, n.Status })
            .HasDatabaseName("IX_NotificationLogs_Channel_Status");

        // Retry job — find failed notifications due for retry
        builder.HasIndex(n => new { n.Status, n.NextRetryAt })
            .HasDatabaseName("IX_NotificationLogs_Status_NextRetryAt");

        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("IX_NotificationLogs_CreatedAt");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(n => !n.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        // UserId nullable — system notifications don't belong to a specific user
        builder.HasOne(n => n.User)
            .WithMany(u => u.NotificationLogs)
            .HasForeignKey(n => n.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}