using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using PcbErpApi.Services;

namespace PcbErpApi.Pages.SPO
{
    public class SPO00003Model : PageModel
    {
        private readonly PcbErpContext _context;
        private readonly PaginationService _pagedService;
        private readonly ITableDictionaryService _dictService;

        public SPO00003Model(PcbErpContext context, PaginationService pagedService, ITableDictionaryService dictService)
        {
            _context = context;
            _pagedService = pagedService;
            _dictService = dictService;
        }

        public string PageTitle => "SPO00003 銷售訂單查詢";

        [BindProperty(SupportsGet = true)]
        public string? PaperNum { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? CustomerId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? CustPONum { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? PartNum { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? PaperDateFrom { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? PaperDateTo { get; set; }

        [BindProperty(SupportsGet = true, Name = "page")]
        public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true, Name = "pageSize")]
        public int PageSize { get; set; } = 50;

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<OrderInqRow> Items { get; set; } = new();

        public List<CURdTableField> QueryDictFields { get; set; } = new();
        public List<CURdTableField> GridDictFields { get; set; } = new();

        public async Task OnGetAsync()
        {
            QueryDictFields = LoadDictSafe("SPOdOrderInqParams");
            GridDictFields = LoadDictSafe("SPOdOrderInq");

            var query =
                from m in _context.SpodOrderMain.AsNoTracking()
                join s in _context.SpodOrderSub.AsNoTracking()
                    on m.PaperNum equals s.PaperNum
                select new OrderInqRow
                {
                    PaperNum = m.PaperNum,
                    PaperDate = m.PaperDate,
                    CustomerId = m.CustomerId,
                    CustPONum = m.CustPonum ?? "",
                    Status = m.Status,
                    MoneyCode = m.MoneyCode,
                    RateToNt = m.RateToNt,
                    Item = s.Item,
                    PartNum = s.PartNum,
                    CustomerPartNum = s.CustomerPartNum,
                    Qnty = s.Qnty,
                    UnitPrice = s.UnitPrice,
                    SubTotal = s.SubTotal,
                    DelDate = s.DelDate
                };

            if (!string.IsNullOrWhiteSpace(PaperNum))
                query = query.Where(x => x.PaperNum.Contains(PaperNum));
            if (!string.IsNullOrWhiteSpace(CustomerId))
                query = query.Where(x => x.CustomerId.Contains(CustomerId));
            if (!string.IsNullOrWhiteSpace(CustPONum))
                query = query.Where(x => x.CustPONum.Contains(CustPONum));
            if (!string.IsNullOrWhiteSpace(PartNum))
                query = query.Where(x => x.PartNum.Contains(PartNum));

            if (PaperDateFrom.HasValue)
                query = query.Where(x => x.PaperDate >= PaperDateFrom.Value.Date);
            if (PaperDateTo.HasValue)
            {
                var end = PaperDateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.PaperDate < end);
            }

            query = query
                .OrderByDescending(x => x.PaperDate)
                .ThenByDescending(x => x.PaperNum)
                .ThenBy(x => x.Item);

            PageNumber = PageNumber <= 0 ? 1 : PageNumber;
            PageSize = PageSize <= 0 ? 50 : Math.Min(PageSize, 500);

            var result = await _pagedService.GetPagedAsync(query, PageNumber, PageSize);
            TotalCount = result.TotalCount;
            Items = result.Data;
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        }

        private List<CURdTableField> LoadDictSafe(string tableName)
        {
            try
            {
                var list = _dictService.GetFieldDict(tableName, typeof(object));
                return list ?? new List<CURdTableField>();
            }
            catch
            {
                return new List<CURdTableField>();
            }
        }

        public string BuildPageUrl(int page)
        {
            var query = new Dictionary<string, string?>
            {
                ["PaperNum"] = PaperNum,
                ["CustomerId"] = CustomerId,
                ["CustPONum"] = CustPONum,
                ["PartNum"] = PartNum,
                ["PaperDateFrom"] = PaperDateFrom?.ToString("yyyy-MM-dd"),
                ["PaperDateTo"] = PaperDateTo?.ToString("yyyy-MM-dd"),
                ["page"] = page.ToString(),
                ["pageSize"] = PageSize.ToString()
            };

            var filtered = query
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .ToDictionary(k => k.Key, v => v.Value!);

            return QueryString.Create(filtered).ToString();
        }

        public class OrderInqRow
        {
            public string PaperNum { get; set; } = "";
            public DateTime PaperDate { get; set; }
            public string CustomerId { get; set; } = "";
            public string CustPONum { get; set; } = "";
            public int Status { get; set; }
            public string StatusText => Status switch
            {
                0 => "未確認",
                1 => "已確認",
                2 => "已結案",
                _ => Status.ToString()
            };
            public byte MoneyCode { get; set; }
            public decimal RateToNt { get; set; }
            public int Item { get; set; }
            public string PartNum { get; set; } = "";
            public string CustomerPartNum { get; set; } = "";
            public decimal Qnty { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal SubTotal { get; set; }
            public DateTime? DelDate { get; set; }
        }
    }
}
