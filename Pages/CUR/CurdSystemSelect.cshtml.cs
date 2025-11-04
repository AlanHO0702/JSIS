using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.CUR
{
    public class CURdSystemSelectModel : PageModel
    {
        private readonly PcbErpContext _db;
        private readonly ITableDictionaryService _dictService;

        public CURdSystemSelectModel(PcbErpContext db, ITableDictionaryService dictService)
        {
            _db = db;
            _dictService = dictService;
        }

        public string TableName => "CURdSystemSelect";

        // ✅ 共用樣板需要的三個屬性
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }

        // ✅ 主資料
        public List<CurdSystemSelect> Items { get; set; } = new();

        // ✅ 辭典欄位
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CURdTableField> TableFields { get; set; } = new();

        public async Task OnGetAsync(int page = 1, int pageSize = 50)
        {
            PageNumber = page;
            PageSize = pageSize;

            // 撈主資料
            var query = _db.CurdSystemSelects.AsNoTracking().OrderBy(x => x.SystemId);
            TotalCount = await query.CountAsync();

            Items = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // 撈辭典
            FieldDictList = _dictService.GetFieldDict("CURdSystemSelect", typeof(CurdSystemSelect));
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
