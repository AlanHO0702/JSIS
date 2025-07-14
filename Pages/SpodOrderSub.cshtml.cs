using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using static SpodOrdersModel;

public class SpodOrderSubModel : TableDetailModel<SpodOrderSub>
{
    public SpodOrderSubModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
        : base(httpClientFactory, dictService) { }

    public override string TableName => "SpodOrderSub";
    public override string ApiDetailUrl => "/api/SpodOrderSub";

    public async Task OnGetAsync(string PaperNum)
    {
        await FetchDataAsync(PaperNum);
    }
}



