using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class PaperInfoModel : PageModel
    {
        private readonly PcbErpContext _context;

        public PaperInfoModel(PcbErpContext context)
        {
            _context = context;
        }

        public List<ModuleNode> Modules { get; set; } = new();
        public Dictionary<string, List<SystemNode>> SystemMap { get; set; } = new();
        public List<CurdPaperInfo> PaperInfos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SelectedSystemId { get; set; }

        public async Task OnGetAsync()
        {
            // 載入模組列表 (Level 0)
            var modules = await _context.CurdModules
                .OrderBy(m => m.ModuleId)
                .ToListAsync();

            Modules = modules.Select(m => new ModuleNode
            {
                ModuleId = m.ModuleId.Trim(),
                ModuleName = m.ModuleName.Trim(),
                DisplayName = $"{m.ModuleId.Trim()}={m.ModuleName.Trim()}"
            }).ToList();

            // 載入系統列表 (Level 1)
            var systems = await _context.CurdVSystems
                .OrderBy(s => s.SystemId)
                .ToListAsync();

            // 將系統按照模組分組
            var systemGroups = systems
                .Select(s => new SystemNode
                {
                    SystemId = s.SystemId.Trim(),
                    SystemName = s.SystemName.Trim(),
                    DisplayName = $"{s.SystemId.Trim()}={s.SystemName.Trim()}",
                    SuperId = s.SystemId.Trim().Substring(0, 1) // 取第一個字元作為 SuperId
                })
                .GroupBy(s => s.SuperId);

            SystemMap = systemGroups.ToDictionary(g => g.Key, g => g.ToList());

            // 載入單據資訊
            if (!string.IsNullOrEmpty(SelectedSystemId))
            {
                PaperInfos = await _context.CurdPaperInfos
                    .Where(p => p.SystemId == SelectedSystemId)
                    .OrderBy(p => p.PaperId)
                    .ToListAsync();
            }
            else
            {
                PaperInfos = await _context.CurdPaperInfos
                    .OrderBy(p => p.SystemId)
                    .ThenBy(p => p.PaperId)
                    .ToListAsync();
            }
        }

        public class ModuleNode
        {
            public string ModuleId { get; set; } = null!;
            public string ModuleName { get; set; } = null!;
            public string DisplayName { get; set; } = null!;
        }

        public class SystemNode
        {
            public string SystemId { get; set; } = null!;
            public string SystemName { get; set; } = null!;
            public string DisplayName { get; set; } = null!;
            public string SuperId { get; set; } = null!;
        }
    }
}
