public class PaginationViewModel
{
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public string PageParameterName { get; set; } = "page";
    public string? RouteUrl { get; set; } // 例如 "/SpodOrders"

    // 若有 QueryString 其它參數可自行加入

    public string GetPageUrl(int page)
    {
        // 自動組合網址，包含 route
        if (string.IsNullOrEmpty(RouteUrl))
            return $"?{PageParameterName}={page}";
        return $"{RouteUrl}?{PageParameterName}={page}";
    }
}
