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

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }

        public List<CurdSystemSelect> Items { get; set; } = new();

        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CURdTableField> TableFields { get; set; } = new();

        public async Task OnGetAsync(int page = 1, int pageSize = 50)
        {
            PageNumber = page;
            PageSize = pageSize;

            var query = _db.CurdSystemSelects.AsNoTracking().OrderBy(x => x.SystemId);
            TotalCount = await query.CountAsync();

            Items = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            FieldDictList = _dictService.GetFieldDict("CURdSystemSelect", typeof(CurdSystemSelect));
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();
            ViewData["OCXLookups"] = _dictService.GetOCXLookups("CURdSystemSelect");

            ViewData["DictTableName"] = TableName;
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
        }
    }
}
