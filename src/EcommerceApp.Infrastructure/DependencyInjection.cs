using EcommerceApp.Application.Common;
using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Infrastructure.Auth;
using EcommerceApp.Infrastructure.Persistence;
using EcommerceApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EcommerceApp.Infrastructure.Notifications;
using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Interfaces;
using EcommerceApp.Infrastructure.Persistence.Repositories;
using EcommerceApp.Application.Common;
using EcommerceApp.Domain.Interfaces;
using EcommerceApp.Infrastructure.BackgroundServices;
using EcommerceApp.Infrastructure.Search;
using Microsoft.Extensions.Options;
using Nest;

namespace EcommerceApp.Infrastructure;

/// <summary>
/// Registers all Infrastructure layer services.
/// Called once from Program.cs: services.AddInfrastructure(configuration);
///
/// ── PARTIAL — this file grows across parts ───────────────────────────────────
/// Part  8: Auth services, repositories, DbContext
/// Part 11: Email providers
/// Part 12: SMS providers, NotificationService
/// Part 15: Search services, Quartz sync job
/// Part 16: Cart background service
/// Part 17: Price drop alert handler
/// Part 19: Stripe payment service, Stock reservation cleanup
/// Part 22: Rate limit service
/// Part 25: Complete final wiring with all remaining services
/// ─────────────────────────────────────────────────────────────────────────────
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddAuthServices(configuration)
            .AddRepositories()
            .AddNotifications(configuration)
            .AddSearch(configuration);

        return services;
    }

    // ── Search ────────────────────────────────────────────────────────────────

    private static IServiceCollection AddSearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind SearchSettings section
        services.Configure<SearchSettings>(
            configuration.GetSection(SearchSettings.SectionName));

        var provider = configuration
            .GetSection(SearchSettings.SectionName)["Provider"] ?? "Sql";

        //Console.WriteLine($"[Search] Provider from config = '{provider}'");

        if (provider.Equals("Elasticsearch", StringComparison.OrdinalIgnoreCase))
        {
            // ── Elasticsearch ─────────────────────────────────────────────────────
            //Console.WriteLine("[Search] Registered ElasticsearchSearchService");
            services.AddSingleton<IElasticClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<SearchSettings>>().Value;

                var connectionSettings = new ConnectionSettings(new Uri(settings.Uri))
                    .DefaultIndex(settings.IndexName)
                    .EnableDebugMode()
                    .PrettyJson();

                // Add credentials only when configured (production)
                if (!string.IsNullOrEmpty(settings.Username) &&
                    !string.IsNullOrEmpty(settings.Password))
                {
                    connectionSettings.BasicAuthentication(
                        settings.Username, settings.Password);
                }

                return new ElasticClient(connectionSettings);
            });

            services.AddScoped<ElasticsearchSearchService>();
            services.AddScoped<ISearchService>(sp =>
                sp.GetRequiredService<ElasticsearchSearchService>());
        }
        else
        {
            //Console.WriteLine("[Search] Registered SqlSearchService");
            // ── SQL fallback ──────────────────────────────────────────────────────
            services.AddScoped<SqlSearchService>();
            services.AddScoped<ISearchService>(sp =>
                sp.GetRequiredService<SqlSearchService>());
        }

        // Background sync service — runs regardless of provider
        // (SQL provider's SyncAllAsync is a no-op, so no wasted work)
        services.AddHostedService<ProductSearchSyncService>();

        return services;
    }

    private static IServiceCollection AddNotifications(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        // Bind "Notifications" section to NotificationSettings
        services.Configure<NotificationSettings>(
            configuration.GetSection(NotificationSettings.SectionName));

        // Dev senders — log to console instead of calling real APIs.
        // In Part 11/12 these are swapped for SendGrid and Twilio
        // by checking IHostEnvironment.IsProduction() or a feature flag.
        services.AddTransient<IEmailSender, DevEmailSender>();
        services.AddTransient<ISmsSender, DevSmsSender>();

        return services;
    }

    // ── Database ──────────────────────────────────────────────────────────────

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    // Retry on transient SQL Server failures (e.g. brief network blips)
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);

                    sqlOptions.MigrationsAssembly(
                        typeof(AppDbContext).Assembly.FullName);
                });
        });

        return services;
    }

    // ── Auth services ─────────────────────────────────────────────────────────

    private static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind "Jwt" section to JwtSettings
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        // Singleton: stateless — all fields are readonly after construction.
        // Safe to share across requests.
        services.AddSingleton<ITokenService, JwtTokenService>();

        // Singleton: stateless (only uses RandomNumberGenerator and BCrypt)
        services.AddSingleton<IOtpService, OtpService>();

        // Singleton: stateless (only uses BCrypt and Regex)
        services.AddSingleton<IPasswordService, PasswordService>();

        // Scoped: wraps ITokenStoreRepository and IUnitOfWork which are both scoped
        services.AddScoped<SessionService>();

        return services;
    }

    // ── Repositories ──────────────────────────────────────────────────────────

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        // Unit of Work — scoped to the HTTP request
        services.AddScoped<IUnitOfWork, Persistence.UnitOfWork>();

        // Auth-specific repos — injected directly into handlers (not via UoW)
        services.AddScoped<ITokenStoreRepository, TokenStoreRepository>();
        services.AddScoped<IOtpStoreRepository, OtpStoreRepository>();

        // Search repo — injected directly into search service and sync job
        services.AddScoped<ISearchRepository, SearchRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();

        // ── ADD this line inside the existing AddRepositories() method ────────────────
        services.AddScoped<IBannerRepository, BannerRepository>();

        // NOTE: The remaining feature repositories are registered automatically
        // through IUnitOfWork lazy initialization (Part 5 UnitOfWork.cs).
        // Individual repos are only registered here when a handler injects
        // them DIRECTLY instead of going through IUnitOfWork.

        return services;
    }
}
