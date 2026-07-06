namespace SharedKernel;

public sealed class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalItems { get; init; }

    public int TotalPages { get; init; }
}
