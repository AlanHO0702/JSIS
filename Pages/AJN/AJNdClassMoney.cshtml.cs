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

    // ★ 核心：儲存「實體欄位名稱」與「顯示名稱」 主檔資料
    public List<AJNdClassMoney> ExchangeRates { get; set; }

    // 新增：被選取的幣別代號（從 query string 傳入）
    [BindProperty(SupportsGet = true)]
    public byte? SelectedMoneyCode { get; set; }

    // 新增：該幣別的匯率歷史資料
    public List<AJNdClassMoneyHis> RateHistories { get; set; } = new();

    public async Task OnGetAsync()
    {
        // 取得所有幣別主檔
        ExchangeRates = await _context.AJNdClassMoney
            .OrderBy(m => m.MoneyCode)
            .ToListAsync();

        // 若有選擇幣別，載入該幣別的匯率歷史
        if (SelectedMoneyCode.HasValue)
        {
            RateHistories = await _context.AJNdClassMoneyHis
                .Where(h => h.MoneyCode == SelectedMoneyCode.Value)
                .OrderByDescending(h => h.RateDate)
                .ToListAsync();
        }
    }
}