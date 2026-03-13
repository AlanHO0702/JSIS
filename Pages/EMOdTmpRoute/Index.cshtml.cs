using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.EMOdTmpRoute
{
    public class IndexModel : TableListModel<EMOdTmpRouteMas>
    {
        private readonly IBreadcrumbService _breadcrumbService;
        private readonly PcbErpContext _ctx;

        public IndexModel(
            IHttpClientFactory httpClientFactory,
            ITableDictionaryService dictService,
            PcbErpContext context,
            ILogger<TableListModel<EMOdTmpRouteMas>> logger,
            IBreadcrumbService breadcrumbService)
            : base(httpClientFactory, dictService, context, logger)
        {
            _breadcrumbService = breadcrumbService;
            _ctx = context;
        }

        public override string TableName => "EMOdTmpRouteMas";

        public override async Task OnGetAsync()
        {
            ViewData["TableTitle"] = "途程主檔";
            ViewData["DictTableName"] = TableName;
            ViewData["KeyFields"] = new[] { "TmpId" };
            ViewData["HideSingleGridToolbar"] = false;

            ViewData["Title"] = "EMO00048 途程主檔";

            var sysItem = await _ctx.CurdSysItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemId == "EMO00048");
            if (sysItem != null)
                ViewData["Breadcrumbs"] = await _breadcrumbService.BuildBreadcrumbsAsync(sysItem.SuperId);

            await base.OnGetAsync();
        }
    }
}
