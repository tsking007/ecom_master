using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");

        builder.HasKey(r => r.Id);

        // ── Properties ────────────────────────────────────────────────────────
        // 1–5 star rating — validated in the application layer
        builder.Property(r => r.Rating).IsRequired();

        builder.Property(r => r.Title)
            .HasMaxLength(200);

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.HelpfulCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // One review per user per product
        builder.HasIndex(r => new { r.UserId, r.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_ProductReviews_UserId_ProductId");

        // Product detail page — load approved reviews
        builder.HasIndex(r => new { r.ProductId, r.IsApproved })
            .HasDatabaseName("IX_ProductReviews_ProductId_IsApproved");

        // Admin moderation queue
        builder.HasIndex(r => r.IsApproved)
            .HasDatabaseName("IX_ProductReviews_IsApproved");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(r => !r.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        // Product FK is configured in ProductConfiguration
    }
}