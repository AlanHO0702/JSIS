using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using static SpodOrdersModel;

public class SpodOrderSubModel : TableDetailModel<SpodOrderSub>
{
    public SpodOrderSubModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
        : base(httpClientFactory, dictService) { }

    public override string TableName => "SPOdOrderSub";
    public override string ApiDetailUrl => "/api/SpodOrderSub";

    public string HeaderTableName => "SPOdOrderMain";


    // 新增這兩個
    public Dictionary<string, object>? HeaderData { get; set; }
    public List<TableFieldViewModel>? HeaderTableFields { get; set; }

    public async Task OnGetAsync(string PaperNum)
    {
        // 1. 抓單頭資料
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        HeaderData = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>($"{baseUrl}/api/SPOdOrderMain/{PaperNum}");

        // 2. 取得單頭欄位字典（可視欄位）
        var headerFieldDicts = _dictService.GetFieldDict("SPOdOrderMain", typeof(SpodOrderMain));

        HeaderTableFields = headerFieldDicts
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .Select(x => new TableFieldViewModel
            {
                FieldName = x.FieldName,
                DisplayLabel = x.DisplayLabel,
                SerialNum = x.SerialNum ?? 0,
                Visible = x.Visible == 1,
                iFieldWidth = x.iFieldWidth ?? 160,  // 加這行！預設 160
                iFieldHeight = x.iFieldHeight ?? 22,
                iFieldTop = x.iFieldTop ?? 0,  // 加這行！預設 160
                iFieldLeft = x.iFieldLeft ?? 0,
                iShowWhere= x.iShowWhere ?? 0    
            }).ToList();


        // 3. 抓單身資料
        await FetchDataAsync(PaperNum);
    }
}




