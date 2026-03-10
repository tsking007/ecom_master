using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
{
    public WishlistRepository(AppDbContext context) : base(context) { }

    // ── User's wishlist ───────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Wishlist>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // ── Single-item lookup ────────────────────────────────────────────────────

    public async Task<Wishlist?> GetByUserAndProductAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                w => w.UserId == userId && w.ProductId == productId,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(
                w => w.UserId == userId && w.ProductId == productId,
                cancellationToken);
    }

    // ── Price-drop alert eligibility ──────────────────────────────────────────

    public async Task<IReadOnlyList<Wishlist>> GetItemsEligibleForPriceDropAlertAsync(
        CancellationToken cancellationToken = default)
    {
        // Find items where:
        //   1. No alert has been sent yet (PriceDropAlertSentAt is null)
        //   2. The product is currently cheaper than when it was wishlisted
        //
        // DiscountedPrice ?? Price is translated to COALESCE(DiscountedPrice, Price) in SQL

        return await _dbSet
            .Include(w => w.Product)
            .Include(w => w.User)
            .Where(w =>
                w.PriceDropAlertSentAt == null &&
                w.Product.IsActive &&
                (
                    // Has a discount and that discount price is below the price when wishlisted
                    (w.Product.DiscountedPrice != null &&
                     w.Product.DiscountedPrice < w.PriceAtAdd)
                    ||
                    // No discount but regular price dropped below wishlist price
                    (w.Product.DiscountedPrice == null &&
                     w.Product.Price < w.PriceAtAdd)
                ))
            .ToListAsync(cancellationToken);
    }
}