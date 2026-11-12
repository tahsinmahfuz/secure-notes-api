namespace Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int Limit { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages => Limit > 0 ? (int)Math.Ceiling((double)TotalCount / Limit) : 0;
}

public static class PaginationDefaults
{
    public static (int Page, int Limit) Normalize(int page, int limit)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedLimit = limit switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => limit
        };

        return (normalizedPage, normalizedLimit);
    }
}

