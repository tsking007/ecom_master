using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;
using EcommerceApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Infrastructure.Persistence.Repositories;

public class NotificationRepository
    : GenericRepository<NotificationLog>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    // ── Retry job ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<NotificationLog>> GetFailedForRetryAsync(
        int maxRetryCount,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .Where(n =>
                n.Status == NotificationStatus.Failed &&
                n.RetryCount < maxRetryCount &&
                n.NextRetryAt != null &&
                n.NextRetryAt <= now)
            .OrderBy(n => n.NextRetryAt)
            .ToListAsync(cancellationToken);
    }

    // ── Bell dropdown ─────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<NotificationLog>> GetPendingForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // "Pending" InApp notifications = unread items in the bell dropdown
        return await _dbSet
            .Where(n =>
                n.UserId == userId &&
                n.Channel == NotificationChannel.InApp &&
                n.Status == NotificationStatus.Pending)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)       // cap the dropdown at 20 items
            .ToListAsync(cancellationToken);
    }

    // ── Customer notification history ─────────────────────────────────────────

    public async Task<PagedResult<NotificationLog>> GetPagedForUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(n =>
                n.UserId == userId &&
                n.Channel == NotificationChannel.InApp);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<NotificationLog>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ── Admin log viewer ──────────────────────────────────────────────────────

    public async Task<PagedResult<NotificationLog>> GetAdminPagedAsync(
        int pageNumber,
        int pageSize,
        NotificationChannel? channel,
        NotificationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(n => n.User).AsQueryable();

        if (channel.HasValue)
            query = query.Where(n => n.Channel == channel.Value);

        if (status.HasValue)
            query = query.Where(n => n.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<NotificationLog>.Create(items, totalCount, pageNumber, pageSize);
    }

    // ── Bell badge count ──────────────────────────────────────────────────────

    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(
                n => n.UserId == userId &&
                     n.Channel == NotificationChannel.InApp &&
                     n.Status == NotificationStatus.Pending,
                cancellationToken);
    }
}