namespace PcbErpApi.Models
{
    /// <summary>
    /// 麵包屑導航項目
    /// </summary>
    public class BreadcrumbItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Url { get; set; }
    }
}
