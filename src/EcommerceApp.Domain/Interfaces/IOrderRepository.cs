using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByStripeSessionIdAsync(
        string stripeSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>Loads the order with all OrderItems for detail / invoice views.</summary>
    Task<Order?> GetWithItemsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    /// <summary>Customer-facing order history — paginated, optionally filtered by status.</summary>
    Task<PagedResult<Order>> GetByUserIdPagedAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        TrackingStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin order list — multi-field filter: tracking status, payment status,
    /// date range, and free-text search on order number / customer email.
    /// </summary>
    Task<PagedResult<Order>> GetAdminPagedAsync(
        int pageNumber,
        int pageSize,
        TrackingStatus? trackingStatus,
        PaymentStatus? paymentStatus,
        DateTime? from,
        DateTime? to,
        string? searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the next human-readable order number in the format ORD-YYYYMMDD-NNNNN.
    /// Uses a DB-level atomic counter or MAX query to avoid duplicates.
    /// </summary>
    Task<string> GenerateOrderNumberAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Returns aggregated revenue and status counts for the admin dashboard.</summary>
    Task<OrderStats> GetStatsAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
}