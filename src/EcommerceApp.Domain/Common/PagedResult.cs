namespace EcommerceApp.Domain.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
    public bool IsEmpty => !Items.Any();

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    // ── Factory methods ───────────────────────────────────────────────────────

    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
        => new(items, totalCount, pageNumber, pageSize);

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
        => new(new List<T>(), 0, pageNumber, pageSize);

    // ── Projection helper ─────────────────────────────────────────────────────

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> mapper)
        => new(Items.Select(mapper).ToList(), TotalCount, PageNumber, PageSize);

    //public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 20)
    //=> new(new List<T>(), 0, pageNumber, pageSize);
}