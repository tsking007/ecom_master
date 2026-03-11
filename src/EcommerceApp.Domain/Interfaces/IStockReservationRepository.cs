using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

public interface IStockReservationRepository : IRepository<StockReservation>
{
    Task<IReadOnlyList<StockReservation>> GetActiveBySessionIdAsync(
        string stripeSessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockReservation>> GetActiveByOrderProductsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}