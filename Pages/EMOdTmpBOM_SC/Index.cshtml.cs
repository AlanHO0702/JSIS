using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.EMOdTmpBOM_SC
{
    public class IndexModel : TableListModel<EMOdTmpBOMMas>
    {
        private readonly IBreadcrumbService _breadcrumbService;
        private readonly PcbErpContext _ctx;

        public IndexModel(
            IHttpClientFactory httpClientFactory,
            ITableDictionaryService dictService,
            PcbErpContext context,
            ILogger<TableListModel<EMOdTmpBOMMas>> logger,
            IBreadcrumbService breadcrumbService)
            : base(httpClientFactory, dictService, context, logger)
        {
            _breadcrumbService = breadcrumbService;
            _ctx = context;
        }

        public override string TableName => "EMOdTmpBOMMas";

        public override async Task OnGetAsync()
        {
            ViewData["TableTitle"] = "組合模型";
            ViewData["DictTableName"] = TableName;
            ViewData["KeyFields"] = new[] { "TmpId" };
            ViewData["HideSingleGridToolbar"] = false;

            ViewData["Title"] = "EMO00047 組合模型";

            var sysItem = await _ctx.CurdSysItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemId == "EMO00047");
            if (sysItem != null)
                ViewData["Breadcrumbs"] = await _breadcrumbService.BuildBreadcrumbsAsync(sysItem.SuperId);

            await base.OnGetAsync();
        }
    }
}
