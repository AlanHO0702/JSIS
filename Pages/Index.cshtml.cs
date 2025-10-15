using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PcbErpContext _context;

        public IndexModel(PcbErpContext context)
        {
            _context = context;
        }

        public List<CurdSysItem> Level0Items { get; set; } = new();
        public Dictionary<string, List<CurdSysItem>> Level1Map { get; set; } = new();
        public Dictionary<string, List<CurdSysItem>> Level2Map { get; set; } = new();
        public string NewlyAddedItemId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedLevel1Id { get; set; } = "";

        public async Task OnGetAsync(string? level1Id = null)
        {
            SelectedLevel1Id = level1Id?.Trim() ?? "";

            // 先把全部項目抓回來
            var items = await _context.CurdSysItems
                .Where(i => i.ItemId != null && i.SystemId != null)
                .ToListAsync();

            // 去尾端空白
            foreach (var i in items)
            {
                i.ItemId   = i.ItemId.Trim();
                i.SystemId = i.SystemId.Trim();
                i.SuperId  = i.SuperId?.Trim();
            }

            // 讀取 CURdSystemSelect 的排序，做成 Dictionary
            var orderPairs = await _context.CurdSystemSelects
                .Select(s => new { SystemId = s.SystemId.Trim(), s.OrderNum })
                .ToListAsync();

            var orderMap = orderPairs.ToDictionary(x => x.SystemId, x => x.OrderNum);

            // Level 0 用 CURdSystemSelect.OrderNum 排序（找不到的排在最後）
            Level0Items = items
                .Where(i => i.LevelNo == 0)
                .OrderBy(i => orderMap.TryGetValue(i.SystemId, out var n) ? n : int.MaxValue)
                .ThenBy(i => i.SerialNum)  // 次要排序保留原本序號
                .ToList();

            // Level 1、Level 2 照原本 SerialNum
            Level1Map = items
                .Where(i => i.LevelNo == 1 && i.SuperId != null)
                .GroupBy(i => i.SuperId!)
                .ToDictionary(g => g.Key, g => g.OrderBy(i => i.SerialNum).ToList());

            Level2Map = items
                .Where(i => i.LevelNo == 2 && i.SuperId != null)
                .GroupBy(i => i.SuperId!)
                .ToDictionary(g => g.Key, g => g.OrderBy(i => i.SerialNum).ToList());
        }

        public List<CurdSysItem> GetCurrentLevel2() =>
            Level2Map.TryGetValue(SelectedLevel1Id, out var list) ? list : new List<CurdSysItem>();
    }
}
