using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

public interface IReviewRepository : IRepository<ProductReview>
{
    /// <summary>
    /// Paginated reviews for a product detail page.
    /// sortBy: "newest" | "highest" | "lowest"
    /// approvedOnly: false for admin queue, true for public view.
    /// </summary>
    Task<PagedResult<ProductReview>> GetByProductIdAsync(
        Guid productId,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool approvedOnly,
        CancellationToken cancellationToken = default);

    /// <summary>Returns null if the user has not yet reviewed this product.</summary>
    Task<ProductReview?> GetByUserAndProductAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has at least one Delivered order containing productId.
    /// Used to auto-set IsVerifiedPurchase on new reviews.
    /// </summary>
    Task<bool> HasVerifiedPurchaseAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>Admin moderation queue — reviews pending approval.</summary>
    Task<PagedResult<ProductReview>> GetUnapprovedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns count of reviews grouped by star rating (1–5).
    /// Used to render the rating breakdown bars on the product detail page.
    /// e.g. { 5: 120, 4: 45, 3: 10, 2: 3, 1: 2 }
    /// </summary>
    Task<Dictionary<int, int>> GetRatingBreakdownAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}