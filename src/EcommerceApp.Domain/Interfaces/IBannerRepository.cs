using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Banner repository — not included in IUnitOfWork because banners
/// are managed independently (admin CRUD + public read).
/// Injected directly into handlers and the Part 14 BannersController.
/// </summary>
public interface IBannerRepository
{
    /// <summary>
    /// Returns IsActive banners within their optional StartDate–EndDate window,
    /// ordered by DisplayOrder ascending.
    /// </summary>
    Task<IReadOnlyList<Banner>> GetActiveBannersAsync(
        CancellationToken cancellationToken = default);

    Task<Banner?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Banner>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Banner banner,
        CancellationToken cancellationToken = default);

    void Update(Banner banner);

    void SoftDelete(Banner banner);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}