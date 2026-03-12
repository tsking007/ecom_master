using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace EcommerceApp.Infrastructure.Persistence;

/// <summary>
/// Ties all repositories to a single AppDbContext instance.
/// Repositories are lazy-initialized — only created on first access.
/// Inject IUnitOfWork when a use-case needs to span multiple repositories
/// in a single atomic operation.
/// Inject individual IRepository implementations directly for single-entity use-cases.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // ── Lazy-initialized backing fields ──────────────────────────────────────
    private IUserRepository? _users;
    private IProductRepository? _products;
    private ICartRepository? _carts;
    private IOrderRepository? _orders;
    private IReviewRepository? _reviews;
    private IWishlistRepository? _wishlists;
    private INotificationRepository? _notifications;
    private IRateLimitRepository? _rateLimits;
    private IStockReservationRepository? _stockReservations;
    private IAddressRepository? _addresses;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    // ── Repository accessors (lazy init) ──────────────────────────────────────

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public ICartRepository Carts =>
        _carts ??= new CartRepository(_context);

    public IOrderRepository Orders =>
        _orders ??= new OrderRepository(_context);

    public IReviewRepository Reviews =>
        _reviews ??= new ReviewRepository(_context);

    public IWishlistRepository Wishlists =>
        _wishlists ??= new WishlistRepository(_context);

    public INotificationRepository Notifications =>
        _notifications ??= new NotificationRepository(_context);

    public IRateLimitRepository RateLimits =>
        _rateLimits ??= new RateLimitRepository(_context);

    public IStockReservationRepository StockReservations =>
        _stockReservations ??= new StockReservationRepository(_context);

    public IAddressRepository Addresses =>
    _addresses ??= new AddressRepository(_context);

    // ── Persistence ───────────────────────────────────────────────────────────

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // ── Transactions ──────────────────────────────────────────────────────────

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException(
                "A transaction is already in progress. " +
                "Nested transactions are not supported.");

        _currentTransaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException(
                "No active transaction to commit.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}