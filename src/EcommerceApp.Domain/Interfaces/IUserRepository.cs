using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<User?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user with all their Address records eagerly loaded.
    /// Used in the profile and checkout flows.
    /// </summary>
    Task<User?> GetWithAddressesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> PhoneExistsAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin user list — supports free-text search on name/email and
    /// filtering by active status.
    /// </summary>
    Task<PagedResult<User>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default);
}