using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.EMOdTmpPress
{
    public class IndexModel : TableListModel<EMOdTmpPressMas>
    {
        private readonly IBreadcrumbService _breadcrumbService;
        private readonly PcbErpContext _ctx;

        public IndexModel(
            IHttpClientFactory httpClientFactory,
            ITableDictionaryService dictService,
            PcbErpContext context,
            ILogger<TableListModel<EMOdTmpPressMas>> logger,
            IBreadcrumbService breadcrumbService)
            : base(httpClientFactory, dictService, context, logger)
        {
            _breadcrumbService = breadcrumbService;
            _ctx = context;
        }

        public override string TableName => "EMOdTmpPressMas";

        public override async Task OnGetAsync()
        {
            ViewData["TableTitle"] = "壓合疊構";
            ViewData["DictTableName"] = TableName;
            ViewData["KeyFields"] = new[] { "TmpId" };
            ViewData["HideSingleGridToolbar"] = false;

            ViewData["Title"] = "EMO00049 壓合疊構";

            var sysItem = await _ctx.CurdSysItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemId == "EMO00049");
            if (sysItem != null)
                ViewData["Breadcrumbs"] = await _breadcrumbService.BuildBreadcrumbsAsync(sysItem.SuperId);

            await base.OnGetAsync();
        }
    }
}
