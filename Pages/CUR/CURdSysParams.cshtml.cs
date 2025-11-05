using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.CUR
{
    public class CURdSysParamsModel : PageModel
    {
        private readonly PcbErpContext _db;
        private readonly ITableDictionaryService _dictService;

        public CURdSysParamsModel(PcbErpContext db, ITableDictionaryService dictService)
        {
            _db = db;
            _dictService = dictService;
        }

        // 主資料表名（不是辭典）
        public string TableName => "CURdSysParams";

        // 共用樣板需要
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 99999;
        public int TotalCount { get; set; }

        // 主資料
        public List<CURdSysParams> Items { get; set; } = new();

        // 辭典欄位
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CURdTableField> TableFields { get; set; } = new();

        public async Task OnGetAsync(int page = 1, int pageSize = 99999)
        {
            PageNumber = page;
            PageSize = pageSize;

            // 撈主資料
            var query = _db.CURdSysParams.AsNoTracking().OrderBy(x => x.SystemId);
            TotalCount = await query.CountAsync();
            Items = await query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync();

            var dictName = "CURdSysParamsDLL";
                
            // 撈辭典（用解析後的同一個表名）
            FieldDictList = _dictService.GetFieldDict(dictName, typeof(CurdSysItem));
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            // 這裡很關鍵：把實際使用的「辭典表名」丟給 View，Modal 存回去就會打在同一張表
            ViewData["DictTableName"] = dictName;   // ← 原本寫 TableName（主資料）就會錯
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"]        = TableFields;
        }
    }
}
