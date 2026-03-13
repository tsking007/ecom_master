using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for the ElasticStore SQL mirror table.
/// Used by the SQL search fallback (SqlSearchService) and the
/// ProductSearchSyncService background job.
/// Registered directly in DI — not part of IUnitOfWork.
/// </summary>
public class SearchRepository : ISearchRepository
{
    private readonly AppDbContext _context;

    public SearchRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Index operations ──────────────────────────────────────────────────────

    public async Task UpsertAsync(
        ElasticStore entry,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.ElasticStores
            .FirstOrDefaultAsync(
                e => e.ProductId == entry.ProductId,
                cancellationToken);

        if (existing == null)
        {
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.LastSyncedAt = DateTime.UtcNow;
            await _context.ElasticStores.AddAsync(entry, cancellationToken);
        }
        else
        {
            // Update all denormalized fields
            existing.Name = entry.Name;
            existing.Description = entry.Description;
            existing.ShortDescription = entry.ShortDescription;
            existing.Category = entry.Category;
            existing.SubCategory = entry.SubCategory;
            existing.Brand = entry.Brand;
            existing.Tags = entry.Tags;
            existing.Price = entry.Price;
            existing.DiscountedPrice = entry.DiscountedPrice;
            existing.AverageRating = entry.AverageRating;
            existing.ReviewCount = entry.ReviewCount;
            existing.SoldCount = entry.SoldCount;
            existing.IsActive = entry.IsActive;
            existing.ThumbnailImageUrl = entry.ThumbnailImageUrl;
            existing.LastSyncedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
        }
    }

    public async Task DeleteAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _context.ElasticStores
            .FirstOrDefaultAsync(
                e => e.ProductId == productId,
                cancellationToken);

        if (entry == null) return;

        //entry.IsActive = false;
        //entry.UpdatedAt = DateTime.UtcNow;

        _context.ElasticStores.Remove(entry);
        //_context.Entry(entry).State = EntityState.Modified;
    }

    // ── SQL search fallback ───────────────────────────────────────────────────

    public async Task<PagedResult<ElasticStore>> SearchAsync(
        string query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var term = query.ToLower().Trim();

        var baseQuery = _context.ElasticStores
            .Where(e => e.IsActive &&
                (e.Name.ToLower().Contains(term) ||
                 e.Description.ToLower().Contains(term) ||
                 (e.Brand != null && e.Brand.ToLower().Contains(term)) ||
                 e.Category.ToLower().Contains(term)));

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(e => e.AverageRating)
            .ThenByDescending(e => e.SoldCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ElasticStore>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ── Full re-sync ──────────────────────────────────────────────────────────

    public async Task SyncAllFromProductsAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            var entry = new ElasticStore
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                ShortDescription = product.ShortDescription,
                Category = product.Category,
                SubCategory = product.SubCategory,
                Brand = product.Brand,
                Tags = product.Tags,
                Price = product.Price,
                DiscountedPrice = product.DiscountedPrice,
                AverageRating = product.AverageRating,
                ReviewCount = product.ReviewCount,
                SoldCount = product.SoldCount,
                IsActive = product.IsActive,
                ThumbnailImageUrl = product.ImageUrls.FirstOrDefault(),
                LastSyncedAt = DateTime.UtcNow
            };

            await UpsertAsync(entry, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}