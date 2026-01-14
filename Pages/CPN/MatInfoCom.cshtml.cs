using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Pages.CPN
{
    public class MatInfoComModel : PageModel
    {
        private readonly PcbErpContext _ctx;

        public MatInfoComModel(PcbErpContext ctx)
        {
            _ctx = ctx;
        }

        public string ItemId { get; private set; } = string.Empty;
        public string ItemName { get; private set; } = "料號主檔";

        public async Task<IActionResult> OnGetAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return NotFound("itemId is required.");

            ItemId = itemId;

            var name = await _ctx.CurdSysItems.AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .Select(x => x.ItemName)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(name))
                ItemName = name;

            return Page();
        }
    }
}
