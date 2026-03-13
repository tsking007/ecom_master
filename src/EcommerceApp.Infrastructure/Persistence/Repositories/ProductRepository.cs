using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    // ── Single-product reads ──────────────────────────────────────────────────

    public async Task<Product?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                p => p.Slug == slug && p.IsActive,
                cancellationToken);
    }

    public async Task<Product?> GetWithReviewsAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    // ── Full catalog query ────────────────────────────────────────────────────

    public async Task<PagedResult<Product>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? category,
        string? subCategory,
        decimal? minPrice,
        decimal? maxPrice,
        double? minRating,
        string? brand,
        string? sortBy,
        bool sortDescending,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // ── Filters ───────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p =>
                p.Category.ToLower() == category.ToLower());

        if (!string.IsNullOrWhiteSpace(subCategory))
            query = query.Where(p =>
                p.SubCategory != null &&
                p.SubCategory.ToLower() == subCategory.ToLower());

        if (minPrice.HasValue)
            query = query.Where(p =>
                (p.DiscountedPrice ?? p.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p =>
                (p.DiscountedPrice ?? p.Price) <= maxPrice.Value);

        if (minRating.HasValue)
            query = query.Where(p => p.AverageRating >= minRating.Value);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(p =>
                p.Brand != null &&
                p.Brand.ToLower() == brand.ToLower());

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        // ── Sorting ───────────────────────────────────────────────────────────
        // EF Core translates (DiscountedPrice ?? Price) → COALESCE(DiscountedPrice, Price)
        //query = sortBy?.ToLower() switch
        //{
        //    "price-asc" => query.OrderBy(p => p.DiscountedPrice ?? p.Price),
        //    "price-desc" => query.OrderByDescending(p => p.DiscountedPrice ?? p.Price),
        //    "rating" => query.OrderByDescending(p => p.AverageRating),
        //    "popular" => query.OrderByDescending(p => p.SoldCount),
        //    "oldest" => query.OrderBy(p => p.CreatedAt),
        //    _ => query.OrderByDescending(p => p.CreatedAt)     // default: newest
        //};
        IOrderedQueryable<Product> orderedQuery = sortBy?.ToLower() switch
        {
            "price" => sortDescending
                ? query.OrderByDescending(p => p.DiscountedPrice ?? p.Price)
                : query.OrderBy(p => p.DiscountedPrice ?? p.Price),

            "rating" => sortDescending
                ? query.OrderByDescending(p => p.AverageRating)
                : query.OrderBy(p => p.AverageRating),

            "soldcount" => sortDescending
                ? query.OrderByDescending(p => p.SoldCount)
                : query.OrderBy(p => p.SoldCount),

            "createdat" => sortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),

            "name" => sortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),

            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Product>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ── Home page sections ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Product>> GetBestsellersAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.SoldCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    // ── Category helpers ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(string Category, int Count)>> GetCategoriesWithCountAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Where(p => p.IsActive)
            .GroupBy(p => p.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderBy(x => x.Category)
            .ToListAsync(cancellationToken);

        return result
            .Select(x => (x.Category, x.Count))
            .ToList();
    }

    // ── Rating recalculation ──────────────────────────────────────────────────

    public async Task UpdateAverageRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var stats = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                AverageRating = g.Average(r => (double)r.Rating),
                ReviewCount = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var product = await _dbSet
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null) return;

        product.AverageRating = stats?.AverageRating ?? 0.0;
        product.ReviewCount = stats?.ReviewCount ?? 0;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Entry(product).State = EntityState.Modified;
    }

    // ── Slug checks ───────────────────────────────────────────────────────────

    public async Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters — a slug used by a soft-deleted product is still reserved
        return await _dbSet
            //.IgnoreQueryFilters()
            .AnyAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<bool> SlugExistsForOtherProductAsync(
        string slug,
        Guid excludeProductId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            //.IgnoreQueryFilters()
            .AnyAsync(
                p => p.Slug == slug && p.Id != excludeProductId,
                cancellationToken);
    }
}