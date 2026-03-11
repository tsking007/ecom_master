using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class StockReservationRepository
    : GenericRepository<StockReservation>, IStockReservationRepository
{
    public StockReservationRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<StockReservation>> GetActiveBySessionIdAsync(
        string stripeSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Product)
            .Where(x =>
                x.StripeSessionId == stripeSessionId &&
                !x.IsReleased)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockReservation>> GetActiveByOrderProductsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
            return Array.Empty<StockReservation>();

        return await _dbSet
            .Include(x => x.Product)
            .Where(x =>
                x.UserId == userId &&
                !x.IsReleased &&
                productIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);
    }
}