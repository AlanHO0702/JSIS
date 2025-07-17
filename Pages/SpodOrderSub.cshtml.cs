using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using static SpodOrdersModel;

public class SpodOrderSubModel : TableDetailModel<SpodOrderSub>
{
    public SpodOrderSubModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
        : base(httpClientFactory, dictService) { }

    public override string TableName => "SPOdOrderSub";
    public override string ApiDetailUrl => "/api/SpodOrderSub";

    // 新增這兩個
    public Dictionary<string, object>? HeaderData { get; set; }
    public List<TableFieldViewModel>? HeaderTableFields { get; set; }

    public async Task OnGetAsync(string PaperNum)
    {
        // 1. 抓單頭資料
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        HeaderData = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>($"{baseUrl}/api/SPOdOrderMain/{PaperNum}");

        // 2. 取得單頭欄位字典（可視欄位）
        HeaderTableFields = _dictService.GetFieldDict("SPOdOrderMain", typeof(SpodOrderMain))
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .Select(x => new TableFieldViewModel
            {
                FieldName = x.FieldName,
                DisplayLabel = x.DisplayLabel,
                SerialNum = x.SerialNum ?? 0,
                Visible = x.Visible == 1
            }).ToList();

        // 3. 抓單身資料
        await FetchDataAsync(PaperNum);
    }
}




