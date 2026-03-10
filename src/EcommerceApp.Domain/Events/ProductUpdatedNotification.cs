namespace EcommerceApp.Domain.Events;

/// <summary>
/// Published when any product field is changed by an admin.
/// Consumed by:
///   - ProductSearchSyncService  → re-indexes the product in Elasticsearch + ElasticStore SQL
///   - PriceDropAlertService     → checks if OldPrice > NewPrice and notifies wishlist/cart users
/// </summary>
public record ProductUpdatedNotification(
    Guid ProductId,
    string Name,
    string Slug,
    decimal OldPrice,
    decimal NewPrice,
    bool IsActive
) : IDomainEvent;