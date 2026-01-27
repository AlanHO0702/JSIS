using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class MenuModel : PageModel
    {
        private readonly PcbErpContext _context;

        public MenuModel(PcbErpContext context)
        {
            _context = context;
        }

        public List<CurdSysItem> Level0Items { get; set; } = new();
        public Dictionary<string, List<CurdSysItem>> Level1Map { get; set; } = new();
        public Dictionary<string, List<CurdSysItem>> Level2Map { get; set; } = new();
        public List<CurdSysItem> SelectedItems { get; set; } = new();
        public HashSet<string> WebMenuHasChildren { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ItemIdToWebMenuId { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string NewlyAddedItemId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedLevel1Id { get; set; } = "";

        public async Task OnGetAsync(string? level1Id = null)
        {
            SelectedLevel1Id = level1Id?.Trim() ?? "";

            var webNodes = await _context.CurdSysItems
                .AsNoTracking()
                .Where(i =>
                    i.SystemId != null &&
                    i.SystemId.Trim().ToUpper() == "WEB" &&
                    i.SWebMenuId != null &&
                    i.IWebEnable == 1 &&
                    i.Enabled == 1)
                .ToListAsync();

            var allItems = await _context.CurdSysItems
                .AsNoTracking()
                .Where(i =>
                    i.SWebMenuId != null &&
                    i.IWebEnable == 1 &&
                    i.Enabled == 1)
                .ToListAsync();

            foreach (var i in webNodes)
            {
                i.ItemId = i.ItemId.Trim();
                i.SystemId = i.SystemId.Trim().ToUpperInvariant();
                i.SuperId = i.SuperId?.Trim();
                i.SWebMenuId = i.SWebMenuId?.Trim();
                i.SWebSuperMenuId = i.SWebSuperMenuId?.Trim();
            }

            foreach (var i in allItems)
            {
                i.ItemId = i.ItemId.Trim();
                i.SystemId = (i.SystemId ?? string.Empty).Trim().ToUpperInvariant();
                i.SuperId = i.SuperId?.Trim();
                i.SWebMenuId = i.SWebMenuId?.Trim();
                i.SWebSuperMenuId = i.SWebSuperMenuId?.Trim();
            }

            webNodes = webNodes
                .Where(i => !string.IsNullOrWhiteSpace(i.SWebMenuId))
                .ToList();

            allItems = allItems
                .Where(i => !string.IsNullOrWhiteSpace(i.SWebMenuId))
                .ToList();

            var childrenMap = allItems
                .Where(i => !string.IsNullOrWhiteSpace(i.SWebSuperMenuId))
                .GroupBy(i => i.SWebSuperMenuId!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            WebMenuHasChildren = new HashSet<string>(childrenMap.Keys, StringComparer.OrdinalIgnoreCase);
            var webChildrenMap = webNodes
                .Where(i => !string.IsNullOrWhiteSpace(i.SWebSuperMenuId))
                .GroupBy(i => i.SWebSuperMenuId!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            List<CurdSysItem> SortWebMenu(IEnumerable<CurdSysItem> list) =>
                list
                    .OrderBy(i => i.IWebMenuOrderSeq ?? long.MaxValue)
                    .ThenBy(i => i.SWebMenuId ?? string.Empty, StringComparer.Ordinal)
                    .ToList();

            var level0List = SortWebMenu(webNodes.Where(i => i.IWebMenuLevel == 1));

            Level0Items = level0List;
            Level1Map = new Dictionary<string, List<CurdSysItem>>(StringComparer.OrdinalIgnoreCase);
            Level2Map = new Dictionary<string, List<CurdSysItem>>(StringComparer.OrdinalIgnoreCase);

            foreach (var root in level0List)
            {
                var key = root.SWebMenuId ?? string.Empty;
                if (!webChildrenMap.TryGetValue(key, out var rawChildren))
                    continue;
                var children = SortWebMenu(rawChildren.Where(i => i.IWebMenuLevel == 2));
                Level1Map[key] = children;

                foreach (var child in children)
                {
                    var childKey = child.SWebMenuId ?? string.Empty;
                    if (!webChildrenMap.TryGetValue(childKey, out var rawGrandChildren))
                        continue;
                    Level2Map[childKey] = SortWebMenu(rawGrandChildren.Where(i => i.IWebMenuLevel == 3));
                }
            }

            if (!string.IsNullOrWhiteSpace(SelectedLevel1Id) &&
                childrenMap.TryGetValue(SelectedLevel1Id, out var selectedChildren))
            {
                SelectedItems = SortWebMenu(selectedChildren);
            }

            ItemIdToWebMenuId = allItems
                .Where(i => !string.IsNullOrWhiteSpace(i.ItemId))
                .GroupBy(i => i.ItemId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => string.IsNullOrWhiteSpace(x.SWebMenuId) ? x.ItemId : x.SWebMenuId)
                          .FirstOrDefault() ?? g.Key,
                    StringComparer.OrdinalIgnoreCase);
        }

        public bool IsWebMenuGroup(CurdSysItem item) =>
            !string.IsNullOrWhiteSpace(item.SWebMenuId) && WebMenuHasChildren.Contains(item.SWebMenuId);
    }
}
