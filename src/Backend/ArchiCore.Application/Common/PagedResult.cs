namespace ArchiCore.Application.Common;

public sealed record PagedResult<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
