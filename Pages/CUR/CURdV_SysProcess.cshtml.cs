using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.CUR
{
    public class CURdV_SysProcess_WEBModel : PageModel
    {
        private readonly PcbErpContext _db;
        private readonly ITableDictionaryService _dictService;

        public CURdV_SysProcess_WEBModel(PcbErpContext db, ITableDictionaryService dictService)
        {
            _db = db;
            _dictService = dictService;
        }

        public string TableName => "CURdV_SysProcess_WEB";

        // ✅ 共用樣板需要的三個屬性
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }

        // ✅ 主資料
        public List<CURdV_SysProcess_WEB> Items { get; set; } = new();

        // ✅ 辭典欄位
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CURdTableField> TableFields { get; set; } = new();

        public async Task OnGetAsync(int page = 1, int pageSize = 50)
        {
            PageNumber = page;
            PageSize = pageSize;

            // 撈主資料
            var query = _db.CURdV_SysProcess_WEB.AsNoTracking().OrderBy(x => x.UserId);
            TotalCount = await query.CountAsync();

            Items = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // 撈辭典
            FieldDictList = _dictService.GetFieldDict("CURdV_SysProcess_WEB", typeof(CURdV_SysProcess_WEB));
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            // 給 ViewData（給共用樣板跟 F3 用）
            ViewData["DictTableName"] = TableName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
        }
    }
}
