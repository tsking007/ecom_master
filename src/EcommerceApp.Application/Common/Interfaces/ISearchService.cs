using EcommerceApp.Application.Features.Search.DTOs;
using EcommerceApp.Domain.Common;

namespace EcommerceApp.Application.Interfaces;

/// <summary>
/// Abstraction over the active search backend.
/// Moved to Application layer because it returns Application-layer DTOs
/// (SearchResultDto). Domain must never reference Application — Clean Architecture.
///
/// Active implementation is selected at startup via configuration:
///   SearchSettings:Provider = "Elasticsearch" | "Sql"
/// </summary>
public interface ISearchService
{
    Task<PagedResult<SearchResultDto>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpsertProductAsync(
        SearchProductDocument product,
        CancellationToken cancellationToken = default);

    Task DeleteProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task SyncAllAsync(CancellationToken cancellationToken = default);

    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Normalised product document used when writing to the search index.
/// Lives in Application because it is produced by Application handlers and
/// consumed by Infrastructure search services.
/// </summary>
public record SearchProductDocument(
    Guid Id,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Brand,
    decimal Price,
    decimal? DiscountedPrice,
    string Category,
    string? SubCategory,
    List<string> Tags,
    string? MainImageUrl,
    double AverageRating,
    int ReviewCount,
    int SoldCount,
    bool IsActive);