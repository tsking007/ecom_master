using AutoMapper;
using AutoMapper.QueryableExtensions;
using EcommerceApp.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Application.Common.Mappings;

/// <summary>
/// Extension methods that project IQueryable sequences and PagedResult objects
/// directly to DTO types without loading every entity into memory.
///
/// Usage in a query handler:
///   var paged = await query.ToPagedResultAsync&lt;Product, ProductListDto&gt;(
///       mapper.ConfigurationProvider, pageNumber, pageSize, cancellationToken);
/// </summary>
public static class MappingExtensions
{
    // ── IQueryable projection ──────────────────────────────────────────────────

    /// <summary>
    /// Applies AutoMapper projection, paginates the query, and returns
    /// a PagedResult&lt;TDto&gt; — all in a single round-trip to the database.
    /// </summary>
    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TSource, TDto>(
        this IQueryable<TSource> query,
        IConfigurationProvider configuration,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ProjectTo<TDto>(configuration)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<TDto>.Create(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Projects an IQueryable directly to a list of DTOs without loading entities.
    /// </summary>
    public static async Task<IReadOnlyList<TDto>> ProjectToListAsync<TDto>(
        this IQueryable<object> query,
        IConfigurationProvider configuration,
        CancellationToken cancellationToken = default)
    {
        return await query
            .ProjectTo<TDto>(configuration)
            .ToListAsync(cancellationToken);
    }

    // ── PagedResult projection ────────────────────────────────────────────────

    /// <summary>
    /// Maps a PagedResult&lt;TSource&gt; to PagedResult&lt;TDto&gt; using AutoMapper.
    /// Preserves pagination metadata (TotalCount, PageNumber, PageSize).
    /// </summary>
    public static PagedResult<TDto> MapPagedResult<TSource, TDto>(
        this PagedResult<TSource> source,
        IMapper mapper)
    {
        var mappedItems = mapper.Map<IReadOnlyList<TDto>>(source.Items);
        return PagedResult<TDto>.Create(
            mappedItems,
            source.TotalCount,
            source.PageNumber,
            source.PageSize);
    }
}