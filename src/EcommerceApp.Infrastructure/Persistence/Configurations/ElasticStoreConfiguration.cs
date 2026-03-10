using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class ElasticStoreConfiguration : IEntityTypeConfiguration<ElasticStore>
{
    public void Configure(EntityTypeBuilder<ElasticStore> builder)
    {
        builder.ToTable("ElasticStores");

        builder.HasKey(e => e.Id);

        // ── Properties ────────────────────────────────────────────────────────
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ShortDescription)
            .HasMaxLength(1000);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.SubCategory)
            .HasMaxLength(100);

        builder.Property(e => e.Brand)
            .HasMaxLength(100);

        var (converter, comparer) = JsonValueConverters.StringList();

        builder.Property(e => e.Tags)
            .HasConversion(converter, comparer)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(e => e.Price)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(e => e.DiscountedPrice)
            .HasColumnType("decimal(18, 2)");

        builder.Property(e => e.ThumbnailImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.LastSyncedAt).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // One row per product — enforced
        builder.HasIndex(e => e.ProductId)
            .IsUnique()
            .HasDatabaseName("IX_ElasticStores_ProductId");

        // SQL fallback search — filter active products by category
        builder.HasIndex(e => new { e.Category, e.IsActive })
            .HasDatabaseName("IX_ElasticStores_Category_IsActive");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_ElasticStores_IsActive");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(e => !e.IsDeleted);
        // Product FK is configured in ProductConfiguration
    }
}