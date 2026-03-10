using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // ── Properties ────────────────────────────────────────────────────────
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18, 2)");

        builder.Property(p => p.DiscountedPrice)
            .HasColumnType("decimal(18, 2)");

        builder.Property(p => p.StockQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ReservedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.SubCategory)
            .HasMaxLength(100);

        builder.Property(p => p.Brand)
            .HasMaxLength(100);

        // ── JSON columns (List<string>) ───────────────────────────────────────
        var (converter, comparer) = JsonValueConverters.StringList();

        builder.Property(p => p.ImageUrls)
            .HasConversion(converter, comparer)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(p => p.Tags)
            .HasConversion(converter, comparer)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        // ─────────────────────────────────────────────────────────────────────
        builder.Property(p => p.VideoUrl)
            .HasMaxLength(500);

        builder.Property(p => p.AverageRating)
            .IsRequired()
            .HasDefaultValue(0.0);

        builder.Property(p => p.ReviewCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.SoldCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.Weight)
            .HasColumnType("decimal(10, 2)");

        builder.Property(p => p.Dimensions)
            .HasMaxLength(100);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        // Computed C# properties — no backing DB columns
        builder.Ignore(p => p.EffectivePrice);
        builder.Ignore(p => p.DiscountPercentage);
        builder.Ignore(p => p.AvailableStock);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Products_Slug");

        // Catalog browse — filter by category, only active products
        builder.HasIndex(p => new { p.Category, p.IsActive })
            .HasDatabaseName("IX_Products_Category_IsActive");

        // Home page featured / bestseller sections
        builder.HasIndex(p => new { p.IsFeatured, p.IsActive })
            .HasDatabaseName("IX_Products_IsFeatured_IsActive");

        builder.HasIndex(p => new { p.IsActive, p.CreatedAt })
            .HasDatabaseName("IX_Products_IsActive_CreatedAt");

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Products_IsDeleted");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(p => !p.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────────
        // Child entities configure their own FK. Defined here to set cascade.
        builder.HasOne(p => p.ElasticStoreEntry)
            .WithOne(e => e.Product)
            .HasForeignKey<ElasticStore>(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.CartItems)
            .WithOne(ci => ci.Product)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.WishlistItems)
            .WithOne(w => w.Product)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.StockReservations)
            .WithOne(s => s.Product)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}