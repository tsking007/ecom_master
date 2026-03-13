using EcommerceApp.Application.Common;
using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EcommerceApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // ── DbSets ────────────────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpStore> OtpStores => Set<OtpStore>();
    public DbSet<TokenStore> TokenStores => Set<TokenStore>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<ElasticStore> ElasticStores => Set<ElasticStore>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<RateLimitLog> RateLimitLogs => Set<RateLimitLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scans this assembly and applies every IEntityTypeConfiguration<T> class.
        // Each configuration is responsible for its own query filter,
        // column types, indexes, and FK relationships.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    // ── Audit field auto-population ───────────────────────────────────────────
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    // Never allow overwriting the original author / timestamp
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}