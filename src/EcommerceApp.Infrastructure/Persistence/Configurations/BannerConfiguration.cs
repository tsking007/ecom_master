using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banners");

        builder.HasKey(b => b.Id);

        // ── Properties ────────────────────────────────────────────────────────
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.SubTitle)
            .HasMaxLength(500);

        builder.Property(b => b.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.LinkUrl)
            .HasMaxLength(500);

        builder.Property(b => b.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        // Carousel query — active banners ordered by display position
        builder.HasIndex(b => new { b.IsActive, b.DisplayOrder })
            .HasDatabaseName("IX_Banners_IsActive_DisplayOrder");

        // ── Soft-delete query filter ──────────────────────────────────────────
        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}