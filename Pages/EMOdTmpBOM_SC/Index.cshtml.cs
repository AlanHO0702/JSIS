using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.EMOdTmpBOM_SC
{
    public class IndexModel : TableListModel<EMOdTmpBOMMas>
    {
        public IndexModel(
            IHttpClientFactory httpClientFactory,
            ITableDictionaryService dictService,
            PcbErpContext context,
            ILogger<TableListModel<EMOdTmpBOMMas>> logger)
            : base(httpClientFactory, dictService, context, logger)
        {
        }

        public override string TableName => "EMOdTmpBOMMas";

        public override async Task OnGetAsync()
        {
            ViewData["TableTitle"] = "組合模型";
            ViewData["DictTableName"] = TableName;
            ViewData["KeyFields"] = new[] { "TmpId" };
            ViewData["HideSingleGridToolbar"] = false;

            await base.OnGetAsync();
        }
    }
}
