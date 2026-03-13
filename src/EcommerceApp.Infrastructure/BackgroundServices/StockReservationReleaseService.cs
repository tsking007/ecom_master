using EcommerceApp.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EcommerceApp.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically scans for expired stock reservations
/// and releases the locked quantity back to the product's available pool.
///
/// Flow per tick:
///   1. Query all StockReservations where IsReleased = false AND ExpiresAt &lt;= UTC now.
///   2. For each reservation:
///        a. Decrement product.ReservedQuantity by reservation.Quantity
///           (clamped to 0 so it never goes negative from a data anomaly).
///        b. Mark reservation.IsReleased = true and set ReleasedAt = UTC now.
///   3. Persist all changes in a single SaveChangesAsync call.
///
/// Concurrency note:
///   Each tick opens its own DI scope and therefore its own AppDbContext,
///   which matches the pattern used by ProductSearchSyncService.
///   If you ever run multiple API instances behind a load balancer you should
///   add a distributed lock (e.g. a SQL application lock or Redis SETNX) around
///   step 1-3 to prevent double-release. For a single-instance deployment this
///   implementation is safe as-is.
/// </summary>
public class StockReservationReleaseService : BackgroundService
{
    // How often the service wakes up and checks for expired reservations.
    // 60 s is a sensible default: worst-case a reservation lingers 60 s past
    // its expiry, which is acceptable for a checkout window of 15+ minutes.
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StockReservationReleaseService> _logger;

    public StockReservationReleaseService(
        IServiceScopeFactory scopeFactory,
        ILogger<StockReservationReleaseService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "StockReservationReleaseService started. " +
            "Checking for expired reservations every {Seconds}s.",
            CheckInterval.TotalSeconds);

        using var timer = new PeriodicTimer(CheckInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await ReleaseExpiredReservationsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown — exit the loop cleanly.
                break;
            }
            catch (Exception ex)
            {
                // Log and continue — a transient DB error must not kill the service.
                _logger.LogError(ex,
                    "Unhandled error in StockReservationReleaseService tick. " +
                    "Will retry in {Seconds}s.",
                    CheckInterval.TotalSeconds);
            }
        }

        _logger.LogInformation("StockReservationReleaseService stopped.");
    }

    // -------------------------------------------------------------------------

    private async Task ReleaseExpiredReservationsAsync(CancellationToken ct)
    {
        // A fresh scope per tick gives us an isolated DbContext, which is
        // required because DbContext is registered as Scoped and BackgroundService
        // lives as a Singleton.
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expired = await unitOfWork.StockReservations.GetExpiredAsync(ct);

        if (expired.Count == 0)
        {
            _logger.LogDebug("No expired stock reservations found.");
            return;
        }

        _logger.LogInformation(
            "Found {Count} expired reservation(s) to release.", expired.Count);

        var now = DateTime.UtcNow;
        int releasedCount = 0;

        foreach (var reservation in expired)
        {
            var product = reservation.Product;

            if (product is null)
            {
                // Shouldn't happen because GetExpiredAsync does .Include(x => x.Product),
                // but guard anyway to avoid a NullReferenceException.
                _logger.LogWarning(
                    "Reservation {ReservationId} has no associated product loaded — skipping.",
                    reservation.Id);
                continue;
            }

            // Clamp to 0: guards against any data inconsistency where
            // ReservedQuantity was already decremented by another path.
            product.ReservedQuantity =
                Math.Max(0, product.ReservedQuantity - reservation.Quantity);

            reservation.IsReleased = true;
            reservation.ReleasedAt = now;

            releasedCount++;

            _logger.LogDebug(
                "Releasing reservation {ReservationId} | " +
                "Product {ProductId} | Qty {Quantity} | " +
                "New ReservedQty {ReservedQty}",
                reservation.Id,
                product.Id,
                reservation.Quantity,
                product.ReservedQuantity);
        }

        // Persist everything in one round-trip.
        await unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Released {ReleasedCount}/{TotalCount} expired reservation(s) successfully.",
            releasedCount, expired.Count);
    }
}