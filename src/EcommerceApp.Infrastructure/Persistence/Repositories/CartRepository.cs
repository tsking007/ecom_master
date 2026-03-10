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
            .Include(c => c.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    // ── Background service ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Cart>> GetIdleCartsAsync(
        int idleDays,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-idleDays);

        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Product)
            .Where(c =>
                c.LastActivityAt < cutoff &&
                c.Items.Any(i => !i.IsDeleted))
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
        // Soft delete the cart item
        item.IsDeleted = true;
        item.UpdatedAt = DateTime.UtcNow;
        _context.Entry(item).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task ClearCartAsync(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        // Batch soft-delete — avoids loading each item individually
        var items = await _context.CartItems
            .Where(ci => ci.CartId == cartId && !ci.IsDeleted)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var item in items)
        {
            item.IsDeleted = true;
            item.UpdatedAt = now;
        }
    }
}