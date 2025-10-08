using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcbErpApi.Pages.SysParams
{
    public class IndexModel : PageModel
    {
        private readonly PcbErpContext _ctx;
        public IndexModel(PcbErpContext ctx) { _ctx = ctx; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedSystem { get; set; }

        public List<string> SystemList { get; set; } = new();
        public List<CURdSysParams> Rows { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1) 先把 SystemId Trim，再過濾掉空白或 null
            SystemList = await _ctx.CURdSysParams
                .Select(x => (x.SystemId ?? "").Trim())
                .Where(s => s != "")
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            // 2) 防呆：把 QueryString 帶入的 SelectedSystem 也 Trim
            SelectedSystem = (SelectedSystem ?? "").Trim();

            // 3) 若沒選或不在清單中 → 預設第一個
            if (string.IsNullOrEmpty(SelectedSystem) || !SystemList.Contains(SelectedSystem))
            {
                SelectedSystem = SystemList.FirstOrDefault();
            }

            // 4) 取清單資料（也做一次 Trim 比對，避免資料庫有尾端空白）
            Rows = string.IsNullOrEmpty(SelectedSystem)
                ? new List<CURdSysParams>()
                : await _ctx.CURdSysParams
                    .Where(x => x.SystemId != null && x.SystemId.Trim() == SelectedSystem)
                    .OrderBy(x => x.ParamId)
                    .AsNoTracking()
                    .ToListAsync();
        }

    }
}
