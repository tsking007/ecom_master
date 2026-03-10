using EcommerceApp.Application.Common;
using EcommerceApp.Domain.Events;
using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcommerceApp.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that keeps the search index in sync with the database.
///
/// THREE sync mechanisms:
///
/// 1. Startup sync (SyncAllAsync)
///    Runs once when the application starts. Rebuilds the entire search index
///    from the Products table. Handles cold starts and schema migrations.
///
/// 2. Event-driven sync (INotificationHandler)
///    Handles three domain events published by Product command handlers:
///      - ProductCreatedNotification → UpsertProductAsync
///      - ProductUpdatedNotification → UpsertProductAsync
///      - ProductDeletedNotification → DeleteProductAsync
///    Keeps the index up-to-date in near real-time after admin changes.
///
/// 3. Scheduled full re-sync (every 24 hours)
///    Catches any drift caused by direct DB edits, failed event handlers,
///    or multi-instance deployments. Runs at 3:00 AM UTC by default.
///    Uses a simple timer loop instead of Quartz to minimise dependencies —
///    replace with a Quartz job in Part 25 if you need cron precision.
/// </summary>
public class ProductSearchSyncService
    : BackgroundService,
      INotificationHandler<ProductCreatedNotification>,
      INotificationHandler<ProductUpdatedNotification>,
      INotificationHandler<ProductDeletedNotification>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SearchSettings _settings;
    private readonly ILogger<ProductSearchSyncService> _logger;

    // Full re-sync interval — 24 hours
    private static readonly TimeSpan ResyncInterval = TimeSpan.FromHours(24);

    public ProductSearchSyncService(
        IServiceScopeFactory scopeFactory,
        IOptions<SearchSettings> settings,
        ILogger<ProductSearchSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    // ── BackgroundService.ExecuteAsync ────────────────────────────────────────

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ProductSearchSyncService starting. Provider: {Provider}",
            _settings.Provider);

        // Startup full sync — runs once immediately
        await RunSyncAllAsync(stoppingToken);

        // Scheduled re-sync every 24 hours
        using var timer = new PeriodicTimer(ResyncInterval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                _logger.LogInformation(
                    "Running scheduled full search re-sync...");
                await RunSyncAllAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Scheduled search re-sync failed — will retry in {Hours}h.",
                    ResyncInterval.TotalHours);
            }
        }
    }

    // ── Domain event handlers ──────────────────────────────────────────────────

    public async Task Handle(
        ProductCreatedNotification notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Indexing newly created product {ProductId} ({Name})",
            notification.ProductId, notification.Name);

        await UpsertFromDatabaseAsync(notification.ProductId, cancellationToken);
    }

    public async Task Handle(
        ProductUpdatedNotification notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Re-indexing updated product {ProductId} ({Name})",
            notification.ProductId, notification.Name);

        await UpsertFromDatabaseAsync(notification.ProductId, cancellationToken);
    }

    public async Task Handle(
        ProductDeletedNotification notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Removing deleted product {ProductId} from search index",
            notification.ProductId);

        using var scope = _scopeFactory.CreateScope();
        var searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        try
        {
            await searchService.DeleteProductAsync(notification.ProductId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to delete product {ProductId} from search index.",
                notification.ProductId);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task RunSyncAllAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        try
        {
            await searchService.SyncAllAsync(ct);
            _logger.LogInformation("Search full sync completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search full sync failed.");
        }
    }

    private async Task UpsertFromDatabaseAsync(
        Guid productId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var searchService = scope.ServiceProvider.GetRequiredService<ISearchService>();

        try
        {
            var product = await unitOfWork.Products.GetByIdAsync(productId, ct);
            if (product == null)
            {
                _logger.LogWarning(
                    "Product {ProductId} not found in DB — skipping index upsert.",
                    productId);
                return;
            }

            var doc = new SearchProductDocument(
                Id: product.Id,
                Name: product.Name,
                Slug: product.Slug,
                ShortDescription: product.ShortDescription,
                Brand: product.Brand,
                Price: product.Price,
                DiscountedPrice: product.DiscountedPrice,
                Category: product.Category,
                SubCategory: product.SubCategory,
                Tags: product.Tags,
                MainImageUrl: product.ImageUrls.FirstOrDefault(),
                AverageRating: product.AverageRating,
                ReviewCount: product.ReviewCount,
                SoldCount: product.SoldCount,
                IsActive: product.IsActive);

            await searchService.UpsertProductAsync(doc, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to upsert product {ProductId} into search index.",
                productId);
        }
    }
}