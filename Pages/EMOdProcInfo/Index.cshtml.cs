using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Helpers;

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
        public List<QueryFieldViewModel> QueryFields { get; set; } = new();

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

            // 查詢欄位 - 使用 iShowWhere = 1 的欄位
            QueryFields = FieldDictList
                .Where(f => f.iShowWhere == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .Select(f => new QueryFieldViewModel
                {
                    ColumnName = f.FieldName ?? "",
                    ColumnCaption = f.DisplayLabel ?? f.FieldName ?? "",
                    DataType = 0,  // 預設為文字類型
                    ControlType = 0  // 預設為一般輸入框
                })
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
