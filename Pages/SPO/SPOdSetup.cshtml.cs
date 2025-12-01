using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.SPO
{
    public class SPOdSetupModel : PageModel
    {
        private readonly PcbErpContext _db;
        private readonly ITableDictionaryService _dictService;

        public SPOdSetupModel(PcbErpContext db, ITableDictionaryService dictService)
        {
            _db = db;
            _dictService = dictService;
        }

        // 🔹 要跟 CURdTableField.TableName 一樣
        public string TableName => "SPODPoKind";

        // 🔹 分頁用
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }

        // 🔹 主資料：改成採購類別主檔 SpodPoKind
        public List<SpodPoKind> Items { get; set; } = new();

        // 🔹 辭典欄位
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CURdTableField> TableFields { get; set; } = new();

        public async Task OnGetAsync(int page = 1, int pageSize = 50)
        {
            PageNumber = page;
            PageSize = pageSize;

            // ✅ 撈 SPODPoKind 主檔資料
            var query = _db.SpodPoKind
                .AsNoTracking()
                .OrderBy(x => x.PoKind);   // 你也可以照需求改排序

            TotalCount = await query.CountAsync();

            Items = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // ✅ 撈欄位辭典：TableName + SpodPoKind 型別
            FieldDictList = _dictService.GetFieldDict(TableName, typeof(SpodPoKind));

            // 只取要顯示的欄位
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            // ✅ 給共用樣板 / F3 用
            ViewData["DictTableName"] = TableName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
        }
    }
}
