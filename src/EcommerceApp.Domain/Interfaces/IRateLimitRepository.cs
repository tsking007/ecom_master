using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Domain.Interfaces;

public interface IRateLimitRepository : IRepository<RateLimitLog>
{
    Task<RateLimitLog?> GetAsync(
        string identifier,
        string action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the existing record or creates and persists a new one if not found.
    /// Used at the start of every rate-limit check to avoid a separate exists call.
    /// </summary>
    Task<RateLimitLog> GetOrCreateAsync(
        string identifier,
        string action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets AttemptCount to 0 and sets WindowStartedAt to UtcNow.
    /// Called when a new rate-limit window begins.
    /// </summary>
    Task ResetWindowAsync(
        string identifier,
        string action,
        CancellationToken cancellationToken = default);
}