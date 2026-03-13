namespace EcommerceApp.Application.Common.Interfaces;

/// <summary>
/// Marker interface. Any IRequest that implements this will be
/// intercepted by CachingBehavior and its result stored in Redis.
/// Only implement this on READ queries — never on commands.
/// </summary>
public interface ICachedQuery
{
    /// <summary>
    /// The Redis key under which the response will be stored.
    /// Must be unique per logical query + its parameters.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// How long the cached value lives before Redis evicts it.
    /// Return null to use the global default from CacheSettings.
    /// </summary>
    TimeSpan? CacheDuration { get; }
}