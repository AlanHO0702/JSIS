using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Pages.CPN
{
    public class MGNdSetNumModel : PageModel
    {
        private readonly PcbErpContext _ctx;

        private const string MainItemId = "MG000006";
        private const string EncodeItemId = "MG000007";

        public string ItemId { get; private set; } = MainItemId;
        public string PageTitle { get; private set; } = "料號主分類";

        public bool IsMainOnly => string.Equals(ItemId, MainItemId, StringComparison.OrdinalIgnoreCase);
        public bool IsEncodeOnly => string.Equals(ItemId, EncodeItemId, StringComparison.OrdinalIgnoreCase);

        public MGNdSetNumModel(PcbErpContext ctx)
        {
            _ctx = ctx;
        }

        public async Task OnGetAsync(string? itemId)
        {
            var routeOrQueryItemId = string.IsNullOrWhiteSpace(itemId)
                ? Request.Query["itemId"].ToString()
                : itemId;

            var resolved = (routeOrQueryItemId ?? string.Empty).Trim().ToUpperInvariant();
            if (resolved != MainItemId && resolved != EncodeItemId)
                resolved = MainItemId;

            ItemId = resolved;

            var fallbackTitle = IsEncodeOnly ? "編碼設定" : "料號主分類";
            var itemName = await _ctx.CurdSysItems.AsNoTracking()
                .Where(x => x.ItemId == ItemId)
                .Select(x => x.ItemName)
                .FirstOrDefaultAsync();

            PageTitle = string.IsNullOrWhiteSpace(itemName) ? fallbackTitle : itemName.Trim();
            ViewData["Title"] = $"{ItemId} {PageTitle}";
        }
    }
}
