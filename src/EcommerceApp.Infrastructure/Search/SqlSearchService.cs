using EcommerceApp.Application.Common;
using EcommerceApp.Application.Features.Search.DTOs;   // ← FIX: was missing
using EcommerceApp.Application.Interfaces;              // ← FIX: moved from Domain
using EcommerceApp.Domain.Common;
using EcommerceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcommerceApp.Infrastructure.Search;

/// <summary>
/// SQL Server fallback search using EF Core LIKE queries.
/// No extra infrastructure required — queries directly against the Products table.
/// Switch to Elasticsearch by setting SearchSettings:Provider = "Elasticsearch".
/// </summary>
public class SqlSearchService : ISearchService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SqlSearchService> _logger;

    public SqlSearchService(
        AppDbContext context,
        ILogger<SqlSearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ── Search ────────────────────────────────────────────────────────────────

    public async Task<PagedResult<SearchResultDto>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return PagedResult<SearchResultDto>.Empty(page, pageSize);

        var lowerTerm = term.ToLower().Trim();

        var query = _context.Products
            .Where(p => p.IsActive)
            .Where(p =>
                p.Name.ToLower().Contains(lowerTerm) ||
                (p.Brand != null && p.Brand.ToLower().Contains(lowerTerm)) ||
                p.Category.ToLower().Contains(lowerTerm) ||
                (p.SubCategory != null && p.SubCategory.ToLower().Contains(lowerTerm)) ||
                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(lowerTerm)));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.Name.ToLower() == lowerTerm)
            .ThenByDescending(p => p.Name.ToLower().StartsWith(lowerTerm))
            .ThenByDescending(p => p.Name.ToLower().Contains(lowerTerm))
            .ThenByDescending(p => p.SoldCount)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new SearchResultDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Brand = p.Brand,
                Price = p.Price,
                DiscountedPrice = p.DiscountedPrice,
                EffectivePrice = p.DiscountedPrice ?? p.Price,
                Category = p.Category,
                SubCategory = p.SubCategory,
                MainImageUrl = p.ImageUrls != null && p.ImageUrls.Count > 0
                    ? p.ImageUrls[0]
                    : null,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                SoldCount = p.SoldCount,
                IsActive = p.IsActive,
                Score = null
            })
            .ToListAsync(cancellationToken);

        return PagedResult<SearchResultDto>.Create(items, totalCount, page, pageSize);
    }

    // ── Index management (no-ops — SQL reads live from Products table) ─────────

    public Task UpsertProductAsync(
        SearchProductDocument product,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task SyncAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "SqlSearchService: SyncAll called — no action needed " +
            "(SQL search queries Products table directly).");
        return Task.CompletedTask;
    }

    public async Task<bool> IsHealthyAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Products.AnyAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}