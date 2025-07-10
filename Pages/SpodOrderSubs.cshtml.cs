using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using static SpodOrdersModel;

public class SpodOrderSubsModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly ITableDictionaryService _dictService;

    public SpodOrderSubsModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _dictService = dictService;
    }

    public string PaperNum { get; set; }
    public List<SpodOrderSub> OrderSubs { get; set; } = new();
    public List<CURdTableField> FieldDictList { get; set; }
    public List<TableFieldViewModel> TableFields { get; set; } = new();
    public async Task OnGetAsync(string PaperNum)
    {
        this.PaperNum = PaperNum;
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var url = $"{baseUrl}/api/SpodOrderSubs?PaperNum={PaperNum}";
        OrderSubs = await _httpClient.GetFromJsonAsync<List<SpodOrderSub>>(url) ?? new();

        FieldDictList = _dictService.GetFieldDict("SpodOrdersub", typeof(SpodOrderSub));
        TableFields = FieldDictList
        .Where(x => x.Visible == 1) // 只取可見欄位
        .OrderBy(x => x.SerialNum)
        .Select(x => new TableFieldViewModel
        {
            FieldName = x.FieldName,
            DisplayLabel = x.DisplayLabel,
            SerialNum = x.SerialNum ?? 0,
            Visible = x.Visible == 1
        }).ToList();
    }
}
