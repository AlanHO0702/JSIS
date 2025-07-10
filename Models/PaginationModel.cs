public class PaginationModel
{
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public string PageParameterName { get; set; } = "page";
    public string? RouteUrl { get; set; } // 例如 "/SpodOrders"
    public string GetPageUrl(int page)
    {
        // 自動組合網址，包含 route
        if (string.IsNullOrEmpty(RouteUrl))
            return $"?{PageParameterName}={page}";
        return $"{RouteUrl}?{PageParameterName}={page}";
    }
}