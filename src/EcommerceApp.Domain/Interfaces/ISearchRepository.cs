using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Repository for the ElasticStore SQL mirror table.
/// This is the persistence layer for the SQL search fallback.
/// The actual search query logic lives in ISearchService implementations.
/// </summary>
public interface ISearchRepository
{
    Task UpsertAsync(
        ElasticStore entry,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ElasticStore>> SearchAsync(
        string query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Full re-sync — copies all active products from Products table
    /// into ElasticStore. Called on startup and by the Quartz 24h job.
    /// </summary>
    Task SyncAllFromProductsAsync(
        CancellationToken cancellationToken = default);
}