using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using WebRazor.Models;

namespace PcbErpApi.Pages.CUR
{
    public class MasterMultiDetailModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        private readonly ILogger<MasterMultiDetailModel> _logger;

        public MasterMultiDetailModel(PcbErpContext ctx, ILogger<MasterMultiDetailModel> logger)
        {
            _ctx = ctx;
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

            ViewData["DictTableName"] = Config.MasterDict ?? Config.MasterTable;
            return Page();
        }
    }
}

