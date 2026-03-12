using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

public interface IAddressRepository : IRepository<Address>
{
    /// <summary>
    /// Returns all non-deleted addresses for a given user, ordered so the
    /// default address always comes first.
    /// </summary>
    Task<IReadOnlyList<Address>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single address that belongs to the specified user.
    /// Returns null when not found or soft-deleted.
    /// </summary>
    Task<Address?> GetByIdAndUserIdAsync(
        Guid addressId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current default address for a user, or null if none is set.
    /// </summary>
    Task<Address?> GetDefaultAddressAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts how many non-deleted addresses the user currently has.
    /// Used to enforce a reasonable per-user address cap.
    /// </summary>
    Task<int> CountByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}