namespace IpBlockApi.Helpers;

public static class PaginationHelper
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public static (int page, int pageSize) Normalize(int? page, int? pageSize)
    {
        var p = page is null or < 1 ? DefaultPage : page.Value;
        var ps = pageSize is null or < 1 ? DefaultPageSize : Math.Min(pageSize.Value, MaxPageSize);
        return (p, ps);
    }
}
