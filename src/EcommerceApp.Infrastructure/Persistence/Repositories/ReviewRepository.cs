using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class ReviewRepository : GenericRepository<ProductReview>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    // ── Product detail page ───────────────────────────────────────────────────

    public async Task<PagedResult<ProductReview>> GetByProductIdAsync(
        Guid productId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool approvedOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(r => r.User)
            .Where(r => r.ProductId == productId);

        if (approvedOnly)
            query = query.Where(r => r.IsApproved);

        // ── Sorting ───────────────────────────────────────────────────────────
        query = sortBy?.ToLower() switch
        {
            "highest" => query.OrderByDescending(r => r.Rating)
                              .ThenByDescending(r => r.CreatedAt),
            "lowest" => query.OrderBy(r => r.Rating)
                              .ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)   // default: newest
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ProductReview>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ── Single-review lookups ─────────────────────────────────────────────────

    public async Task<ProductReview?> GetByUserAndProductAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                r => r.UserId == userId && r.ProductId == productId,
                cancellationToken);
    }

    // ── Verified purchase check ───────────────────────────────────────────────

    public async Task<bool> HasVerifiedPurchaseAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        // Check for a delivered order containing the product
        return await _context.Orders
            .Where(o =>
                o.UserId == userId &&
                o.TrackingStatus == TrackingStatus.Delivered)
            .SelectMany(o => o.Items)
            .AnyAsync(
                oi => oi.ProductId == productId,
                cancellationToken);
    }

    // ── Admin moderation queue ────────────────────────────────────────────────

    public async Task<PagedResult<ProductReview>> GetUnapprovedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => !r.IsApproved);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ProductReview>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ── Rating breakdown ──────────────────────────────────────────────────────

    public async Task<Dictionary<int, int>> GetRatingBreakdownAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var rawCounts = await _dbSet
            .Where(r => r.ProductId == productId && r.IsApproved)
            .GroupBy(r => r.Rating)
            .Select(g => new { Star = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Ensure all 5 stars are represented — fill missing stars with 0
        var breakdown = Enumerable.Range(1, 5)
            .ToDictionary(
                star => star,
                star => rawCounts.FirstOrDefault(x => x.Star == star)?.Count ?? 0);

        return breakdown;
    }
}