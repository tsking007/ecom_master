using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the cart with all CartItems and their Products in one query.
    /// Used in cart display and pre-checkout validation.
    /// </summary>
    Task<Cart?> GetWithItemsAndProductsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns carts where LastActivityAt is older than idleDays.
    /// Used by the CartReminderService background job.
    /// </summary>
    Task<IReadOnlyList<Cart>> GetIdleCartsAsync(
        int idleDays,
        CancellationToken cancellationToken = default);

    Task<CartItem?> GetCartItemAsync(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken = default);

    Task AddCartItemAsync(
        CartItem item,
        CancellationToken cancellationToken = default);

    Task RemoveCartItemAsync(
        CartItem item,
        CancellationToken cancellationToken = default);

    /// <summary>Removes all CartItems belonging to this cart in one operation.</summary>
    Task ClearCartAsync(
        Guid cartId,
        CancellationToken cancellationToken = default);
    
    Task ClearPurchasedItemsAsync(
        Guid cartId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}