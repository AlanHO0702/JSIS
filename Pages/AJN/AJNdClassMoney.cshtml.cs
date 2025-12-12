// /Pages/Shared/TableListModel.cs
using Microsoft.AspNetCore.Mvc; // 導入 MVC 相關功能
using Microsoft.AspNetCore.Mvc.RazorPages; // 導入 Razor Pages
using System.Data;
using PcbErpApi.Data;
using PcbErpApi.Models; // 導入專案模型
using Microsoft.EntityFrameworkCore;

// 幣別匯率主檔的頁面模型
public class AJNdClassMoneyModel : PageModel
{   // 開始類別定義
    private readonly IConfiguration _configuration; // 用於讀取配置
    private readonly PcbErpContext _context;
    private const string DefaultUseId = "a001";
    public AJNdClassMoneyModel(IConfiguration configuration, PcbErpContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public List<CURdTableField> FieldDictList { get; set; } // 欄位定義清單
    public List<string> Columns { get; set; } = new();
    public DataTable TableData { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string TableName { get; set; }
    public string SortColumn { get; set; }
    public string SortDirection { get; set; } = "ASC";

    // ★ 核心：儲存「實體欄位名稱」與「顯示名稱」 主檔資料
    public List<AJNdClassMoney> ExchangeRates { get; set; }

    // 被選取的幣別代號（從 query string 傳入）
    [BindProperty(SupportsGet = true)]
    public byte? SelectedMoneyCode { get; set; }

    // 該幣別的匯率歷史資料
    public List<AJNdClassMoneyHis> RateHistories { get; set; } = new();

    public async Task OnGetAsync()
    {
        // 取得所有幣別主檔
        ExchangeRates = await _context.AJNdClassMoney
            .Where(m => m.UseId == DefaultUseId)
            .OrderBy(m => m.MoneyCode)
            .ToListAsync();

        // 若有選擇幣別，載入該幣別的匯率歷史
        if (SelectedMoneyCode.HasValue)
        {
            var query = _context.AJNdClassMoneyHis
                .Where(h => h.MoneyCode == SelectedMoneyCode.Value && h.UseId == DefaultUseId)
                .OrderByDescending(h => h.RateDate);

            RateHistories = await query.ToListAsync();
        }
    }
}
