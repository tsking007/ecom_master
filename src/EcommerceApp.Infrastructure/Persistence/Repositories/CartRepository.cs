using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    public CartRepository(AppDbContext context) : base(context) { }

    // ── Cart lookups ──────────────────────────────────────────────────────────

    public async Task<Cart?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task<Cart?> GetWithItemsAndProductsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    // ── Background service ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Cart>> GetIdleCartsAsync(
        int idleDays,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .ToListAsync(cancellationToken);
    }

    // ── CartItem operations ───────────────────────────────────────────────────

    public async Task<CartItem?> GetCartItemAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(
                ci => ci.CartId == cartId && ci.ProductId == productId,
                cancellationToken);
    }

    public async Task AddCartItemAsync(
        CartItem item,
        CancellationToken cancellationToken = default)
    {
        await _context.CartItems.AddAsync(item, cancellationToken);
    }

    public Task RemoveCartItemAsync(
        CartItem item,
        CancellationToken cancellationToken = default)
    {
        _context.CartItems.Remove(item);
        return Task.CompletedTask;
    }

    public async Task ClearCartAsync(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        var items = await _context.CartItems
            .Where(ci => ci.CartId == cartId)
            .ToListAsync(cancellationToken);

        _context.CartItems.RemoveRange(items);

    }

    public async Task ClearPurchasedItemsAsync(
        Guid cartId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return;

        var items = await _context.CartItems
            .Where(ci =>
                ci.CartId == cartId && 
                productIds.Contains(ci.ProductId))
            .ToListAsync(cancellationToken);

        _context.CartItems.RemoveRange(items);
    }
}