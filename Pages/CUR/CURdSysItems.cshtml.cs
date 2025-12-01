using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.CUR
{
    public class CURdSysItemsModel : PageModel
    {
       private readonly PcbErpContext _db;
        private readonly ITableDictionaryService _dictService;

        public CURdSysItemsModel(PcbErpContext db, ITableDictionaryService dictService)
        {
            _db = db;
            _dictService = dictService;
        }

        public string TableName => "CURdSysItems";

        // ? �@�μ˪O�ݭn���T���ݩ�
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 99999;
        public int TotalCount { get; set; }

        // ? �D���
        public List<CurdSysItem> Items { get; set; } = new();

        // ? ������
        public List<CURdTableField> FieldDictList { get; set; } = new();
        public List<CURdTableField> TableFields { get; set; } = new();

        public async Task OnGetAsync(int page = 1, int pageSize = 99999)
        {
            PageNumber = page;
            PageSize = pageSize;

            // ���D���
            var query = _db.CurdSysItems.AsNoTracking().OrderBy(x => x.SystemId);
            TotalCount = await query.CountAsync();

            Items = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // 辭典
            FieldDictList = _dictService.GetFieldDict("CURdSysItemsLang", typeof(CurdSysItem));
            TableFields = FieldDictList
                .Where(f => f.Visible == 1)
                .OrderBy(f => f.SerialNum ?? 0)
                .ToList();

            // OCX lookup 來源（以 CURdSysItemsLang 為主，對應後台設定）
            ViewData["OCXLookups"] = _dictService.GetOCXLookups("CURdSysItemsLang");

            // 設定 ViewData，供 F3 對應
            ViewData["DictTableName"] = "CURdSysItemsLang";
            ViewData["FieldDictList"] = FieldDictList;
            ViewData["Fields"] = TableFields;
        }
    }
}

