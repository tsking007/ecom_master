using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    /// <summary>Loads product with its approved reviews for the detail page.</summary>
    Task<Product?> GetWithReviewsAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Full catalog query — every catalog filter, sort, and pagination param.
    /// Null parameters are ignored (treated as "no filter").
    /// </summary>
    Task<PagedResult<Product>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? category,
        string? subCategory,
        decimal? minPrice,
        decimal? maxPrice,
        double? minRating,
        string? brand,
        string? sortBy,
        bool sortDescending,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetBestsellersAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Returns each category paired with how many active products it has.</summary>
    Task<IReadOnlyList<(string Category, int Count)>> GetCategoriesWithCountAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates and persists AverageRating and ReviewCount
    /// from the ProductReviews table for the given product.
    /// Called after every review create / delete.
    /// </summary>
    Task UpdateAverageRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the slug is used by any product OTHER than excludeProductId.
    /// Used during product update to allow keeping the same slug.
    /// </summary>
    Task<bool> SlugExistsForOtherProductAsync(
        string slug,
        Guid excludeProductId,
        CancellationToken cancellationToken = default);
}