using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;

public class SpodOrdersModel : PageModel
{
    private readonly HttpClient _httpClient;

    public SpodOrdersModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public List<SpodOrderMain>? Orders { get; set; }

    public async Task OnGetAsync()
    {
        Orders = await _httpClient.GetFromJsonAsync<List<SpodOrderMain>>("http://localhost:5290/api/SpodOrderMains");
        Orders = Orders?.OrderByDescending(o => o.PaperDate).ToList();
    }
    public string GetStatusName(int status)
{
    return status switch
    {
        0 => "作業中",
        1 => "已確認",
        2 => "已作廢",
        3 => "審核中",
        4 => "已結案"
    };
}

public string GetStatusColor(int status)
{
    return status switch
    {
        1 => "success",    // 已確認
        2 => "danger",    // 已作廢
        4 => "primary",  // 已結案
        _ => "light"       // 草稿 或 未知
    };
}

}

