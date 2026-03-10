using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Generic repository contract. All entity-specific repositories extend this.
/// Soft delete is the default delete path.
/// Hard delete is provided for housekeeping tasks only.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    // ── Single-entity reads ───────────────────────────────────────────────────

    Task<T?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // ── Collection reads ──────────────────────────────────────────────────────

    Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default);

    // ── Writes ────────────────────────────────────────────────────────────────

    Task<T> AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        T entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets IsDeleted = true and UpdatedAt = UtcNow.
    /// The EF global query filter will hide this row from all future queries.
    /// </summary>
    Task SoftDeleteAsync(
        T entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently removes the row from the database.
    /// Use only for housekeeping background jobs.
    /// </summary>
    Task HardDeleteAsync(
        T entity,
        CancellationToken cancellationToken = default);

    // ── Existence / count ─────────────────────────────────────────────────────

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        CancellationToken cancellationToken = default);

    // ── Raw query access ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns an IQueryable for building complex queries inside repository
    /// implementations. Do NOT expose this through the application layer.
    /// </summary>
    IQueryable<T> Query();
}