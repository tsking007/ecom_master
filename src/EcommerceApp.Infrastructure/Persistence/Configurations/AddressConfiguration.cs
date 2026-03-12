using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);

        // ── Properties ────────────────────────────────────────────────────────
        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.AddressLine1)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AddressLine2)
            .HasMaxLength(200);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.AddressType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Home");

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_Addresses_UserId");

        // Default address lookup — used in checkout pre-population
        builder.HasIndex(a => new { a.UserId, a.IsDefault })
            .HasDatabaseName("IX_Addresses_UserId_IsDefault");

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}