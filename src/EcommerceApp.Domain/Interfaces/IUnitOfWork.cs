namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Wraps all repositories and the database transaction into one coherent unit.
/// Inject IUnitOfWork when a single use-case needs to touch multiple repositories
/// and they must all succeed or all fail together.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ── Repository accessors ──────────────────────────────────────────────────

    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ICartRepository Carts { get; }
    IOrderRepository Orders { get; }
    IReviewRepository Reviews { get; }
    IWishlistRepository Wishlists { get; }
    INotificationRepository Notifications { get; }
    IRateLimitRepository RateLimits { get; }

    IStockReservationRepository StockReservations { get; }

    // ── Persistence ───────────────────────────────────────────────────────────

    /// <summary>
    /// Flushes all tracked changes to the database in a single round-trip.
    /// Returns the number of rows affected.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ── Transactions ──────────────────────────────────────────────────────────

    /// <summary>Opens an explicit database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the open transaction.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the open transaction on failure.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}