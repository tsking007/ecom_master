using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Interfaces;

public interface INotificationRepository : IRepository<NotificationLog>
{
    /// <summary>
    /// Returns failed notification logs that have been retried fewer than
    /// maxRetryCount times and whose NextRetryAt is in the past.
    /// Used by the notification retry background job.
    /// </summary>
    Task<IReadOnlyList<NotificationLog>> GetFailedForRetryAsync(
        int maxRetryCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns unread in-app notifications for the bell dropdown.
    /// </summary>
    Task<IReadOnlyList<NotificationLog>> GetPendingForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>Customer-facing paginated notification history.</summary>
    Task<PagedResult<NotificationLog>> GetPagedForUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Admin notification log viewer with channel and status filters.</summary>
    Task<PagedResult<NotificationLog>> GetAdminPagedAsync(
        int pageNumber,
        int pageSize,
        NotificationChannel? channel,
        NotificationStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the count shown on the notification bell badge.</summary>
    Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}