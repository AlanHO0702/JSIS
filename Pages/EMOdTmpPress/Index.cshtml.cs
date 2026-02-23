using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.EMOdTmpPress
{
    public class IndexModel : TableListModel<EMOdTmpPressMas>
    {
        public IndexModel(
            IHttpClientFactory httpClientFactory,
            ITableDictionaryService dictService,
            PcbErpContext context,
            ILogger<TableListModel<EMOdTmpPressMas>> logger)
            : base(httpClientFactory, dictService, context, logger)
        {
        }

        public override string TableName => "EMOdTmpPressMas";

        public override async Task OnGetAsync()
        {
            ViewData["TableTitle"] = "壓合疊構";
            ViewData["DictTableName"] = TableName;
            ViewData["KeyFields"] = new[] { "TmpId" };
            ViewData["HideSingleGridToolbar"] = false;

            await base.OnGetAsync();
        }
    }
}
