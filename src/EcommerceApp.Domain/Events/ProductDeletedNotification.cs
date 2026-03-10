namespace EcommerceApp.Domain.Events;

/// <summary>
/// Published when a product is soft-deleted by an admin.
/// Consumed by:
///   - ProductSearchSyncService  → removes the document from Elasticsearch
///                                 and marks ElasticStore row as inactive
/// </summary>
public record ProductDeletedNotification(
    Guid ProductId,
    string Name,
    string Slug
) : IDomainEvent;