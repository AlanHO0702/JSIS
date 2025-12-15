using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class FontIndexModel : PageModel
    {
        private readonly PcbErpContext _context;

        public FontIndexModel(PcbErpContext context)
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

            // 1. 查所有 CurdSystemSelect（Selected==1）
            var enabledSystemSelects = await _context.CurdSystemSelects
                .Where(s => s.Selected == 1)
                .Select(s => new { s.SystemId, s.OrderNum })
                .ToListAsync();

            var enabledSystemIdSet = enabledSystemSelects.Select(x => x.SystemId.Trim().ToUpper()).ToHashSet();

            // 2. 查所有 CURdSysItem
            var items = await _context.CurdSysItems
                .Where(i => i.ItemId != null && i.SystemId != null)
                .ToListAsync();

            foreach (var i in items)
            {
                i.ItemId = i.ItemId.Trim();
                i.SystemId = i.SystemId.Trim().ToUpper();
                i.SuperId = i.SuperId?.Trim();
            }

            // 3. Level0Items 按 CurdSystemSelect.OrderNum 排序
            var level0List = items
                .Where(i => i.LevelNo == 0 && i.Enabled == 1 && enabledSystemIdSet.Contains(i.SystemId))
                .Join(enabledSystemSelects,
                    l0 => l0.SystemId,
                    sel => sel.SystemId.Trim().ToUpper(),
                    (l0, sel) => new { Item = l0, OrderNum = sel.OrderNum })
                .OrderBy(x => x.OrderNum)
                .Select(x => x.Item)
                .ToList();

            var level0Ids = level0List.Select(x => x.ItemId).ToHashSet();

            // 4. Level1、Level2
            var level1List = items
                .Where(i => i.LevelNo == 1 && i.Enabled == 1 && i.SuperId != null && level0Ids.Contains(i.SuperId))
                .OrderBy(i => i.SerialNum)
                .ToList();
            var level1Ids = level1List.Select(x => x.ItemId).ToHashSet();

            var level2List = items
                .Where(i => i.LevelNo == 2 && i.Enabled == 1 && i.SuperId != null && level1Ids.Contains(i.SuperId))
                .Where(i => i.ItemType != 1) // 不顯示 ITEMTYPE=1 的項目
                .OrderBy(i => i.SerialNum)
                .ToList();

            Level0Items = level0List;
            Level1Map = level1List.GroupBy(i => i.SuperId!).ToDictionary(g => g.Key, g => g.ToList());
            Level2Map = level2List.GroupBy(i => i.SuperId!).ToDictionary(g => g.Key, g => g.ToList());
        }
        public List<CurdSysItem> GetCurrentLevel2() =>
            Level2Map.TryGetValue(SelectedLevel1Id, out var list) ? list : new List<CurdSysItem>();
    }
}
