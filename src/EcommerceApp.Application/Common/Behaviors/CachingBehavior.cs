using EcommerceApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EcommerceApp.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that transparently caches responses
/// for any IRequest that also implements ICachedQuery.
///
/// Pipeline order (outermost → innermost):
///   LoggingBehavior
///     → PerformanceBehavior
///         → CachingBehavior   ← sits here, before validation is fine
///             → ValidationBehavior
///                 → Handler
///
/// Non-ICachedQuery requests pass straight through with zero overhead.
/// </summary>
public class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    // Default TTL used when ICachedQuery.CacheDuration returns null.
    // Kept short-ish so stale bestseller data never lives too long.
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only cache requests that opt-in via ICachedQuery.
        // All other requests (commands, non-cached queries) pass through untouched.
        if (request is not ICachedQuery cachedQuery)
            return await next();

        var cacheKey = cachedQuery.CacheKey;
        var ttl = cachedQuery.CacheDuration ?? DefaultTtl;

        // --- 1. Try to read from cache ---
        try
        {
            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (cached is not null)
            {
                _logger.LogInformation(
                    "[Cache HIT]  key={CacheKey}  type={RequestType}",
                    cacheKey,
                    typeof(TRequest).Name);

                // Deserialize and return — DB is never touched
                return JsonSerializer.Deserialize<TResponse>(cached, JsonOptions)!;
            }
        }
        catch (Exception ex)
        {
            // Redis being unavailable must NEVER break the request.
            // Log the problem and fall through to the real handler.
            _logger.LogWarning(ex,
                "[Cache READ ERROR]  key={CacheKey}  Falling back to DB.",
                cacheKey);

            return await next();
        }

        // --- 2. Cache miss: call the real handler ---
        _logger.LogInformation(
            "[Cache MISS]  key={CacheKey}  type={RequestType}  Querying DB.",
            cacheKey,
            typeof(TRequest).Name);

        var response = await next();

        // --- 3. Store the fresh result in Redis ---
        try
        {
            var serialized = JsonSerializer.Serialize(response, JsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await _cache.SetStringAsync(cacheKey, serialized, options, cancellationToken);

            _logger.LogInformation(
                "[Cache SET]  key={CacheKey}  ttl={TTL}",
                cacheKey,
                ttl);
        }
        catch (Exception ex)
        {
            // A Redis write failure must not fail the response to the client.
            // The user still gets their data; the next request will just miss cache again.
            _logger.LogWarning(ex,
                "[Cache WRITE ERROR]  key={CacheKey}  Response returned uncached.",
                cacheKey);
        }

        return response;
    }
}