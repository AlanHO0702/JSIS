using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.FME
{
    public class FME00011Model : PageModel
    {
        private readonly PcbErpContext _context;
        private readonly IBreadcrumbService _breadcrumbService;

        public FME00011Model(PcbErpContext context, IBreadcrumbService breadcrumbService)
        {
            _context = context;
            _breadcrumbService = breadcrumbService;
        }

        public string ItemId { get; private set; } = "FME00011";
        public string ItemName { get; private set; } = "缺料轉請購作業";

        public async Task<IActionResult> OnGetAsync(bool stay = false)
        {
            var item = await _context.CurdSysItems
                .AsNoTracking()
                .Where(x => x.ItemId == ItemId)
                .Select(x => new { x.ItemName, x.SuperId })
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(item?.ItemName))
                ItemName = item.ItemName!;

            if (!string.IsNullOrWhiteSpace(item?.SuperId))
                ViewData["Breadcrumbs"] = await _breadcrumbService.BuildBreadcrumbsAsync(item.SuperId!);

            if (!stay)
                return Redirect($"/DynamicTemplate/Paper/{ItemId}");

            return Page();
        }
    }
}
