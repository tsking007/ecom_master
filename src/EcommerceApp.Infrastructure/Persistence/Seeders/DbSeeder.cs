using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcommerceApp.Infrastructure.Persistence.Seeders;

/// <summary>
/// Idempotent seeder — safe to call on every startup.
/// Seeds only when the target table is empty.
/// Called from Program.cs after the database is migrated.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await SeedAdminUserAsync(context, logger, cancellationToken);
        await SeedBannersAsync(context, logger, cancellationToken);
        await SeedProductsAsync(context, logger, cancellationToken);
    }

    // ── Admin User ────────────────────────────────────────────────────────────

    private static async Task SeedAdminUserAsync(
        AppDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Role == UserRole.Admin, cancellationToken))
            return;

        logger.LogInformation("Seeding admin user...");

        // BCrypt work factor 12 — change password via admin panel after first login
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234", workFactor: 12);

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Super",
            LastName = "Admin",
            Email = "admin@ecommerceapp.com",
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
            IsEmailVerified = true,
            IsPhoneVerified = false,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Admin user created. Email: admin@ecommerceapp.com | Password: Admin@1234");
    }

    // ── Banners ───────────────────────────────────────────────────────────────

    private static async Task SeedBannersAsync(
        AppDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await context.Banners.AnyAsync(cancellationToken))
            return;

        logger.LogInformation("Seeding banners...");

        var banners = new List<Banner>
        {
            new()
            {
                Id           = Guid.NewGuid(),
                Title        = "Summer Sale — Up to 50% Off",
                SubTitle     = "Shop the biggest deals of the season",
                ImageUrl     = "https://placehold.co/1200x400/FF6B6B/FFFFFF?text=Summer+Sale",
                LinkUrl      = "/products?category=Electronics",
                DisplayOrder = 1,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            },
            new()
            {
                Id           = Guid.NewGuid(),
                Title        = "New Arrivals — Latest Tech",
                SubTitle     = "Discover the newest gadgets and devices",
                ImageUrl     = "https://placehold.co/1200x400/4ECDC4/FFFFFF?text=New+Arrivals",
                LinkUrl      = "/products?sortBy=newest",
                DisplayOrder = 2,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            },
            new()
            {
                Id           = Guid.NewGuid(),
                Title        = "Free Shipping on Orders Over $50",
                SubTitle     = "Use code FREESHIP at checkout",
                ImageUrl     = "https://placehold.co/1200x400/45B7D1/FFFFFF?text=Free+Shipping",
                LinkUrl      = "/products",
                DisplayOrder = 3,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            }
        };

        context.Banners.AddRange(banners);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} banners.", banners.Count);
    }

    // ── Products ──────────────────────────────────────────────────────────────

    private static async Task SeedProductsAsync(
        AppDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken))
            return;

        logger.LogInformation("Seeding sample products...");

        var products = new List<Product>
        {
            // ── Electronics ───────────────────────────────────────────────────
            new()
            {
                Id               = Guid.NewGuid(),
                Name             = "ProMax Smartphone X1",
                Slug             = "promax-smartphone-x1",
                Description      = "The ProMax X1 delivers a stunning 6.7-inch AMOLED display, " +
                                   "a 108 MP triple camera system, and a blazing-fast octa-core processor. " +
                                   "With 5G connectivity and a 5000 mAh battery, stay connected all day long.",
                ShortDescription = "6.7\" AMOLED | 108 MP | 5G | 5000 mAh",
                Price            = 999.99m,
                DiscountedPrice  = 849.99m,
                StockQuantity    = 150,
                Category         = "Electronics",
                SubCategory      = "Smartphones",
                Brand            = "ProMax",
                ImageUrls        = new List<string>
                {
                    "https://placehold.co/600x600/1a1a2e/FFFFFF?text=Phone+Front",
                    "https://placehold.co/600x600/16213e/FFFFFF?text=Phone+Back",
                    "https://placehold.co/600x600/0f3460/FFFFFF?text=Phone+Side"
                },
                Tags             = new List<string> { "smartphone", "5g", "promax", "android" },
                AverageRating    = 4.5,
                ReviewCount      = 128,
                SoldCount        = 342,
                IsActive         = true,
                IsFeatured       = true,
                Weight           = 0.215m,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            },
            new()
            {
                Id               = Guid.NewGuid(),
                Name             = "UltraBook Pro 15",
                Slug             = "ultrabook-pro-15",
                Description      = "The UltraBook Pro 15 combines a razor-thin 14.9 mm chassis with " +
                                   "Intel Core i7, 16 GB DDR5 RAM, and a 1 TB NVMe SSD. " +
                                   "The 15.6-inch 4K OLED display makes every pixel count.",
                ShortDescription = "Intel i7 | 16 GB RAM | 1 TB SSD | 4K OLED",
                Price            = 1499.99m,
                DiscountedPrice  = null,
                StockQuantity    = 75,
                Category         = "Electronics",
                SubCategory      = "Laptops",
                Brand            = "UltraBook",
                ImageUrls        = new List<string>
                {
                    "https://placehold.co/600x600/2d2d2d/FFFFFF?text=Laptop+Open",
                    "https://placehold.co/600x600/3d3d3d/FFFFFF?text=Laptop+Closed"
                },
                Tags             = new List<string> { "laptop", "ultrabook", "intel", "4k", "oled" },
                AverageRating    = 4.7,
                ReviewCount      = 89,
                SoldCount        = 201,
                IsActive         = true,
                IsFeatured       = true,
                Weight           = 1.6m,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            },
            new()
            {
                Id               = Guid.NewGuid(),
                Name             = "SoundWave Pro Headphones",
                Slug             = "soundwave-pro-headphones",
                Description      = "Industry-leading active noise cancellation meets studio-grade " +
                                   "sound quality. Up to 30 hours of battery life, fast charging, " +
                                   "and premium memory-foam ear cushions for all-day comfort.",
                ShortDescription = "ANC | 30h Battery | Hi-Res Audio",
                Price            = 349.99m,
                DiscountedPrice  = 279.99m,
                StockQuantity    = 200,
                Category         = "Electronics",
                SubCategory      = "Audio",
                Brand            = "SoundWave",
                ImageUrls        = new List<string>
                {
                    "https://placehold.co/600x600/1a1a1a/FFFFFF?text=Headphones+Front",
                    "https://placehold.co/600x600/2a2a2a/FFFFFF?text=Headphones+Side"
                },
                Tags             = new List<string> { "headphones", "anc", "wireless", "audio" },
                AverageRating    = 4.8,
                ReviewCount      = 256,
                SoldCount        = 789,
                IsActive         = true,
                IsFeatured       = false,
                Weight           = 0.25m,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            },

            // ── Books ─────────────────────────────────────────────────────────
            new()
            {
                Id               = Guid.NewGuid(),
                Name             = "Clean Code: A Handbook of Agile Software Craftsmanship",
                Slug             = "clean-code-robert-martin",
                Description      = "Even bad code can function. But if code isn't clean, it can bring " +
                                   "a development organization to its knees. This book is a must-read for " +
                                   "any developer who wants to write better, more maintainable software.",
                ShortDescription = "By Robert C. Martin | Paperback | 431 Pages",
                Price            = 49.99m,
                DiscountedPrice  = 39.99m,
                StockQuantity    = 300,
                Category         = "Books",
                SubCategory      = "Programming",
                Brand            = "Prentice Hall",
                ImageUrls        = new List<string>
                {
                    "https://placehold.co/600x600/FFFFFF/000000?text=Clean+Code"
                },
                Tags             = new List<string> { "programming", "software", "agile", "best-practices" },
                AverageRating    = 4.9,
                ReviewCount      = 512,
                SoldCount        = 1204,
                IsActive         = true,
                IsFeatured       = true,
                Weight           = 0.65m,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            },
            new()
            {
                Id               = Guid.NewGuid(),
                Name             = "Designing Data-Intensive Applications",
                Slug             = "designing-data-intensive-applications",
                Description      = "The big ideas behind reliable, scalable, and maintainable systems. " +
                                   "This book examines the pros and cons of various technologies for processing " +
                                   "and storing data — helping you understand the tradeoffs.",
                ShortDescription = "By Martin Kleppmann | Hardcover | 616 Pages",
                Price            = 59.99m,
                DiscountedPrice  = null,
                StockQuantity    = 180,
                Category         = "Books",
                SubCategory      = "Database",
                Brand            = "O'Reilly Media",
                ImageUrls        = new List<string>
                {
                    "https://placehold.co/600x600/E8E8E8/000000?text=DDIA"
                },
                Tags             = new List<string> { "databases", "distributed-systems", "backend" },
                AverageRating    = 4.8,
                ReviewCount      = 320,
                SoldCount        = 876,
                IsActive         = true,
                IsFeatured       = false,
                Weight           = 0.9m,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            },

            // ── Clothing ──────────────────────────────────────────────────────
            new()
            {
                Id               = Guid.NewGuid(),
                Name             = "Classic Oxford Button-Down Shirt",
                Slug             = "classic-oxford-button-down-shirt",
                Description      = "Timeless Oxford weave in 100% premium cotton. Tailored slim fit " +
                                   "with a button-down collar. Machine washable and built to last " +
                                   "through years of everyday wear.",
                ShortDescription = "100% Cotton | Slim Fit | Machine Washable",
                Price            = 79.99m,
                DiscountedPrice  = 59.99m,
                StockQuantity    = 500,
                Category         = "Clothing",
                SubCategory      = "Men's Tops",
                Brand            = "ClassicWear",
                ImageUrls        = new List<string>
                {
                    "https://placehold.co/600x600/4A90E2/FFFFFF?text=Oxford+Shirt+Front",
                    "https://placehold.co/600x600/357ABD/FFFFFF?text=Oxford+Shirt+Back"
                },
                Tags             = new List<string> { "shirt", "cotton", "formal", "men" },
                AverageRating    = 4.3,
                ReviewCount      = 98,
                SoldCount        = 412,
                IsActive         = true,
                IsFeatured       = false,
                Weight           = 0.3m,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            },
            new()
            {
                Id               = Guid.NewGuid(),
                Name             = "Premium Merino Wool Sweater",
                Slug             = "premium-merino-wool-sweater",
                Description      = "Crafted from 100% Merino wool, this sweater offers exceptional " +
                                   "warmth without the bulk. The naturally breathable fibers regulate " +
                                   "temperature to keep you comfortable all day.",
                ShortDescription = "100% Merino Wool | Regular Fit | Hand Wash",
                Price            = 129.99m,
                DiscountedPrice  = null,
                StockQuantity    = 250,
                Category         = "Clothing",
                SubCategory      = "Knitwear",
                Brand            = "WoolCraft",
                ImageUrls        = new List<string>
                {
                    "https://placehold.co/600x600/8B4513/FFFFFF?text=Merino+Sweater"
                },
                Tags             = new List<string> { "sweater", "wool", "merino", "knitwear" },
                AverageRating    = 4.6,
                ReviewCount      = 64,
                SoldCount        = 189,
                IsActive         = true,
                IsFeatured       = true,
                Weight           = 0.45m,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded {Count} sample products.", products.Count);
    }
}