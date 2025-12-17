using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.EMOdProcInfo
{
    public class IndexModel : PageModel
    {
        private readonly PcbErpContext _db;
        private readonly ITableDictionaryService _dictService;

        public IndexModel(PcbErpContext db, ITableDictionaryService dictService)
        {
            _db = db;
            _dictService = dictService;
        }

        public string TableName => "EMOdProcInfo";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 99999;
        public int TotalCount { get; set; }

        public List<EmodProcInfo> Items { get; set; } = new();
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CURdTableField> TableFields { get; set; } = new();

        public async Task OnGetAsync(int page = 1, int pageSize = 99999)
        {
            PageNumber = page;
            PageSize = pageSize;

            // 查詢資料
            var query = _db.EmodProcInfos.AsNoTracking().OrderBy(x => x.ProcCode);
            TotalCount = await query.CountAsync();

            Items = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // 辭典設定 - 使用 FME_EMOdProcInfoPCB 作為辭典名稱
            var dictTableName = "FME_EMOdProcInfoPCB";
            FieldDictList = _dictService.GetFieldDict(dictTableName, typeof(EmodProcInfo));
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            // 設定 ViewData
            ViewData["DictTableName"] = dictTableName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
            ViewData["OCXLookups"] = _dictService.GetOCXLookups(dictTableName);
            ViewData["KeyFields"] = new[] { "ProcCode" };
        }
    }
}
