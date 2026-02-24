using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.EMOdTmpRoute
{
    public class IndexModel : TableListModel<EMOdTmpRouteMas>
    {
        public IndexModel(
            IHttpClientFactory httpClientFactory,
            ITableDictionaryService dictService,
            PcbErpContext context,
            ILogger<TableListModel<EMOdTmpRouteMas>> logger)
            : base(httpClientFactory, dictService, context, logger)
        {
        }

        public override string TableName => "EMOdTmpRouteMas";

        public override async Task OnGetAsync()
        {
            ViewData["TableTitle"] = "途程主檔";
            ViewData["DictTableName"] = TableName;
            ViewData["KeyFields"] = new[] { "TmpId" };
            ViewData["HideSingleGridToolbar"] = false;

            await base.OnGetAsync();
        }
    }
}