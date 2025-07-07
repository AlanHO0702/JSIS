using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;

public class SpodOrdersModel : PageModel
{
    private readonly HttpClient _httpClient;

    public SpodOrdersModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public List<SpodOrderMain> Orders { get; set; } = new();
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((TotalCount) / (double)PageSize);

    public async Task OnGetAsync([FromQuery(Name = "page")] int? page)
    {
        PageNumber = page ?? 1;
        var apiUrl = $"http://localhost:5290/api/SPOdOrderMains/paged?page={PageNumber}&pageSize={PageSize}";
        var resp = await _httpClient.GetFromJsonAsync<ApiResult>(apiUrl);
        Orders = resp?.data ?? new List<SpodOrderMain>();
        TotalCount = resp?.totalCount ?? 0;
    }

    // API Response 物件
    public class ApiResult
    {
        public int totalCount { get; set; }
        public List<SpodOrderMain>? data { get; set; }
    }

    public string GetStatusName(int status)
    {
        return status switch
        {
            0 => "作業中",
            1 => "已確認",
            2 => "已作廢",
            3 => "審核中",
            4 => "已結案",
            _ => "未知"
        };
    }

    public string GetStatusColor(int status)
    {
        return status switch
        {
            1 => "success",
            2 => "danger",
            4 => "primary",
            _ => "light"
        };
    }
}
