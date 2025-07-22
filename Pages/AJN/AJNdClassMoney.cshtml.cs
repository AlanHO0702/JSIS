// /Pages/Shared/TableListModel.cs
using Microsoft.AspNetCore.Mvc; // 導入 MVC 相關功能
using Microsoft.AspNetCore.Mvc.RazorPages; // 導入 Razor Pages
using System.Data;
using PcbErpApi.Data;
using PcbErpApi.Models; // 導入專案模型
using Microsoft.EntityFrameworkCore;

public class AJNdClassMoneyModel : PageModel
{ // 開始類別定義
    private readonly IConfiguration _configuration; // 用於讀取配置
    private readonly PcbErpContext _context;
    public AJNdClassMoneyModel(IConfiguration configuration, PcbErpContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public int PageSize { get; set; } = 50; // 每頁筆數預設為 50
    public int PageNumber { get; set; } = 1; // 目前頁碼，預設 1
    public int TotalCount { get; set; } // 總資料筆數
    public int TotalPages => (int)Math.Ceiling((TotalCount) / (double)PageSize); // 計算總頁數
    public List<CURdTableField> FieldDictList { get; set; } // 欄位定義清單
    public List<string> Columns { get; set; } = new();
    public DataTable TableData { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string TableName { get; set; }

    public string SortColumn { get; set; }
    public string SortDirection { get; set; } = "ASC";

    // ★ 核心：儲存「實體欄位名稱」與「顯示名稱」
    public List<AJNdClassMoney> ExchangeRates { get; set; }

    public async Task OnGetAsync()
    {
        ExchangeRates = await _context.AJNdClassMoney.ToListAsync();
    }
}