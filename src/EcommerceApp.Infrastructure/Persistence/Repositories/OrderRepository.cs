using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    // ── Single-order lookups ──────────────────────────────────────────────────

    public async Task<Order?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefaultAsync(
                o => o.OrderNumber == orderNumber,
                cancellationToken);
    }

    public async Task<Order?> GetByIdForUserAsync(
    Guid orderId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(
                x => x.Id == orderId && x.UserId == userId,
                cancellationToken);
    }

    public async Task<Order?> GetByStripeSessionIdAsync(
        string stripeSessionId,
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters — webhook must process even if order was soft-deleted
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(
                o => o.StripeSessionId == stripeSessionId,
                cancellationToken);
    }

    public async Task<Order?> GetWithItemsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    // ── Customer paginated order history ──────────────────────────────────────

    public async Task<PagedResult<Order>> GetByUserIdPagedAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        TrackingStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(o => o.Items)
            .Where(o => o.UserId == userId);

        if (status.HasValue)
            query = query.Where(o => o.TrackingStatus == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Order>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ── Admin paginated order list ────────────────────────────────────────────

    public async Task<PagedResult<Order>> GetAdminPagedAsync(
        int pageNumber,
        int pageSize,
        TrackingStatus? trackingStatus,
        PaymentStatus? paymentStatus,
        DateTime? from,
        DateTime? to,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(o => o.User)
            .AsQueryable();

        // ── Filters ───────────────────────────────────────────────────────────
        if (trackingStatus.HasValue)
            query = query.Where(o => o.TrackingStatus == trackingStatus.Value);

        if (paymentStatus.HasValue)
            query = query.Where(o => o.PaymentStatus == paymentStatus.Value);

        if (from.HasValue)
            query = query.Where(o => o.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(o => o.CreatedAt <= to.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower().Trim();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                o.User.Email.ToLower().Contains(term) ||
                (o.User.FirstName + " " + o.User.LastName)
                    .ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Order>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ── Order number generation ───────────────────────────────────────────────

    public async Task<string> GenerateOrderNumberAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");

        // Find the last order created today to get the current sequence
        var lastOrderNumber = await _dbSet
            .IgnoreQueryFilters()     // include soft-deleted to keep numbering consistent
            .Where(o => o.CreatedAt.Date == today)
            .OrderByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastOrderNumber != null)
        {
            // Format: ORD-YYYYMMDD-NNNNN → split on '-', take last segment
            var parts = lastOrderNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var last))
                sequence = last + 1;
        }

        return $"ORD-{dateStr}-{sequence:D5}";
    }

    // ── Admin dashboard stats ─────────────────────────────────────────────────

    public async Task<OrderStats> GetStatsAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (from.HasValue)
            query = query.Where(o => o.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(o => o.CreatedAt <= to.Value.AddDays(1));

        // Run revenue and count queries in parallel
        var totalOrdersTask = query.CountAsync(cancellationToken);

        var totalRevenueTask = query
            .Where(o => o.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken);

        var avgOrderValueTask = query
            .AverageAsync(o => (decimal?)o.TotalAmount, cancellationToken);

        // Status breakdown — one group-by instead of multiple Count queries
        var statusCountsTask = query
            .GroupBy(o => o.TrackingStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        await Task.WhenAll(
            totalOrdersTask,
            totalRevenueTask,
            avgOrderValueTask,
            statusCountsTask);

        var statusCounts = statusCountsTask.Result;

        int CountByStatus(TrackingStatus s) =>
            statusCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

        return new OrderStats(
            TotalRevenue: totalRevenueTask.Result ?? 0m,
            TotalOrders: totalOrdersTask.Result,
            PendingOrders: CountByStatus(TrackingStatus.Placed),
            ProcessingOrders:
                CountByStatus(TrackingStatus.Processing) +
                CountByStatus(TrackingStatus.Confirmed),
            ShippedOrders:
                CountByStatus(TrackingStatus.Shipped) +
                CountByStatus(TrackingStatus.OutForDelivery),
            DeliveredOrders: CountByStatus(TrackingStatus.Delivered),
            CancelledOrders: CountByStatus(TrackingStatus.Cancelled),
            AverageOrderValue: avgOrderValueTask.Result ?? 0m
        );
    }
}