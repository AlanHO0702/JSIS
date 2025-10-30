using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.CUR
{
    // ✅ 直接繼承共用模板 TableSingleGrid
    public class CurdBuModel : TableSingleGrid<CurdBu>
    {
        // ✅ 指定對應資料表名稱（這會自動套用在字典查詢與 API 呼叫中）
        public override string TableName => "CURdBU";

        // 建構子注入服務
        public CurdBuModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
            : base(httpClientFactory, dictService)
        {
        }

        // ✅ OnGetAsync 可直接沿用父類別實作，不用再寫
        // 若要加額外初始化，也可以覆寫：
        public override async Task OnGetAsync([FromQuery(Name = "page")] int? page)
        {
            await base.OnGetAsync(page);
            // 可以在這裡加其他初始邏輯
        }
    }
}
