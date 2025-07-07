using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;

public class SpodOrderSubsModel : PageModel
{
    private readonly HttpClient _httpClient;

    public SpodOrderSubsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public string PaperNum { get; set; }
    public List<SpodOrderSub> OrderSubs { get; set; } = new();

    public async Task OnGetAsync(string PaperNum)
    {
        this.PaperNum = PaperNum;
        var url = $"http://localhost:5290/api/SpodOrderSubs?PaperNum={PaperNum}";
        OrderSubs = await _httpClient.GetFromJsonAsync<List<SpodOrderSub>>(url) ?? new();
    }
}
