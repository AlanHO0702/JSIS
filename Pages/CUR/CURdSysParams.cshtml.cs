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

        public string TableName => "CURdSysParams";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 99999;
        public int TotalCount { get; set; }

        public List<CURdSysParams> Items { get; set; } = new();
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CURdTableField> TableFields { get; set; } = new();

        public async Task OnGetAsync(int page = 1, int pageSize = 99999)
        {
            PageNumber = page;
            PageSize = pageSize;

            var query = _db.CURdSysParams.AsNoTracking().OrderBy(x => x.SystemId);
            TotalCount = await query.CountAsync();
            Items = await query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync();

            var dictName = "CURdSysParamsDLL";
            FieldDictList = _dictService.GetFieldDict(dictName, typeof(CURdSysParams));
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            ViewData["DictTableName"] = dictName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"]        = TableFields;
            ViewData["OCXLookups"]    = _dictService.GetOCXLookups(dictName);
        }
    }
}
