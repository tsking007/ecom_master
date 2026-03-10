namespace EcommerceApp.Domain.Events;

/// <summary>
/// Published when a new product is created by an admin.
/// Consumed by:
///   - ProductSearchSyncService  → indexes the product in Elasticsearch + ElasticStore SQL
/// </summary>
public record ProductCreatedNotification(
    Guid ProductId,
    string Name,
    string Slug,
    string Category,
    decimal Price,
    bool IsActive
) : IDomainEvent;