using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

public interface IWishlistRepository : IRepository<Wishlist>
{
    Task<IReadOnlyList<Wishlist>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Wishlist?> GetByUserAndProductAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns wishlist items where the current product price is lower
    /// than PriceAtAdd and PriceDropAlertSentAt is null.
    /// Used by PriceDropAlertService to find users to notify.
    /// </summary>
    Task<IReadOnlyList<Wishlist>> GetItemsEligibleForPriceDropAlertAsync(
        CancellationToken cancellationToken = default);
}