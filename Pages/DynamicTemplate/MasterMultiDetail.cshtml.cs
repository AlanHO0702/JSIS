using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Services;
using WebRazor.Models;

namespace PcbErpApi.Pages.CUR
{
    public class MasterMultiDetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly IBreadcrumbService _breadcrumbService;
        private readonly ILogger<MasterMultiDetailModel> _logger;

        public MasterMultiDetailModel(PcbErpContext ctx, IBreadcrumbService breadcrumbService, ILogger<MasterMultiDetailModel> logger)
        {
            _ctx = ctx;
            _breadcrumbService = breadcrumbService;
            _logger = logger;
        }

        public string ItemId { get; private set; } = string.Empty;
        public string? ItemName { get; private set; }
        public MasterMultiDetailConfig? Config { get; private set; }
        public string? LoadError { get; private set; }

        public async Task<IActionResult> OnGetAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return NotFound("itemId is required.");

            var result = await MasterMultiDetailConfigLoader.LoadAsync(_ctx, _logger, itemId);
            ItemId = result.ItemId;
            ItemName = result.ItemName;
            Config = result.Config;
            LoadError = result.Error;

            if (Config == null)
                return NotFound(result.Error ?? "Master-multi-detail config not found.");

            try
            {
                var superId = await _ctx.CurdSysItems.AsNoTracking()
                    .Where(x => x.ItemId == ItemId)
                    .Select(x => x.SuperId)
                    .SingleOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(superId))
                    ViewData["Breadcrumbs"] = await _breadcrumbService.BuildBreadcrumbsAsync(superId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Build breadcrumbs failed for {ItemId}", ItemId);
            }

            ViewData["DictTableName"] = Config.MasterDict ?? Config.MasterTable;
            return Page();
        }
    }
}
