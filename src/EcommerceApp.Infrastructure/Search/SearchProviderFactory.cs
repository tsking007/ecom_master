using EcommerceApp.Application.Common;
using EcommerceApp.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EcommerceApp.Infrastructure.Search;

/// <summary>
/// Resolves the active ISearchService implementation at runtime based on
/// SearchSettings:Provider in appsettings.json.
///
/// Switching search backends:
///   1. Change SearchSettings:Provider to "Elasticsearch" or "Sql"
///   2. Restart the application — no code changes needed
///
/// The factory is registered as a singleton and relies on the service
/// locator pattern (IServiceProvider) to resolve the correct scoped service.
/// </summary>
public class SearchProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SearchSettings _settings;

    public SearchProviderFactory(
        IServiceProvider serviceProvider,
        IOptions<SearchSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    public ISearchService GetActiveProvider()
    {
        return _settings.Provider.Equals(
            "Elasticsearch", StringComparison.OrdinalIgnoreCase)
            ? _serviceProvider.GetRequiredService<ElasticsearchSearchService>()
            : _serviceProvider.GetRequiredService<SqlSearchService>();
    }
}