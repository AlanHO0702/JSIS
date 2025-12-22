using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using WebRazor.Models;

namespace PcbErpApi.Pages.AJN
{
    /// <summary>
    /// 損益表設定 - 四層級主從式編輯頁面（BalanceSheet 佈局）
    /// 對應 Delphi 的 BalanceDLL.pas (paperType=0)
    /// </summary>
    public class AJNdBalanceModel : PageModel
    {
        private readonly ILogger<AJNdBalanceModel> _logger;

        public AJNdBalanceModel(ILogger<AJNdBalanceModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 頁面標題
        /// </summary>
        public string PageTitle => "損益表設定";

        /// <summary>
        /// 主表格名稱（用於相容性）
        /// </summary>
        public string TableName => "AJNdIncome";

        // 為了相容 View 中的 PaginationModel，提供預設值
        public int PageNumber => 1;
        public int TotalPages => 1;
        public List<CURdTableField> FieldDictList { get; set; } = new List<CURdTableField>();

        public void OnGet()
        {
            // 設定四層級主從式結構（BalanceSheet 佈局）
            var config = new MasterMultiDetailConfig
            {
                DomId = "balance",

                // ========== TopSelector 設定 ==========
                TopSelectorTitle = "損益項目",
                TopSelectorTable = "AJNdIncomeGroup",
                TopSelectorDict = "AJNdIncomeGroup",
                TopSelectorHeight = 150,
                TopSelectorPkFields = new List<string> { "SerialNum", "UseId" },

                // ========== Master 設定 ==========
                MasterTitle = "損益表項目設定",
                MasterTable = "AJNdIncome",
                MasterDict = "AJNdIncome",
                MasterTop = 200,
                MasterPkFields = new List<string> { "SerialNum", "ClassType", "UseId" },

                // TopSelector → Master 關聯鍵
                MasterToTopKeyMap = new List<KeyMapMulti>
                {
                    new KeyMapMulti { Master = "SerialNum", Detail = "SerialNum" },
                    new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                },

                // ========== Layout 設定 ==========
                Layout = LayoutMode.BalanceSheet,
                EnableDetailFocusCascade = false, // 關閉串聯聯動，改為平行聯動
                EnableTopToolbar = true, // 啟用「項目新增」工具列
                EnableSplitters = true, // 啟用拖曳器功能
                EnableGridCounts = true, // 啟用表格計數顯示

                // ========== Detail 設定 ==========
                Details = new List<WebRazor.Models.DetailConfig>
                {
                    // 層級 1：科目明細（對應 Delphi 的 qryDetail1 / AJNdIncomeAccId）
                    // ★ 與 Master 直接聯動
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "損益表科目設定",
                        DetailTable = "AJNdIncomeAccId",
                        DetailDict = "AJNdIncomeAccId",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "SerialNum", Detail = "SerialNum" },
                            new KeyMapMulti { Master = "ClassType", Detail = "ClassType" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                        },
                        PkFields = new List<string> { "SerialNum", "ClassType", "AccId", "SubAccId", "UseId" }
                    },

                    // 層級 2：結轉科目（對應 Delphi 的 qryReverse / AJNdInComeAccIdReverse，可選顯示）
                    // ★ 與 Master 直接聯動（不再依賴 Detail）
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "匯總排除項目",
                        DetailTable = "AJNdInComeAccIdReverse",
                        DetailDict = "AJNdInComeAccIdReverse",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "SerialNum", Detail = "SerialNum" },
                            new KeyMapMulti { Master = "ClassType", Detail = "ClassType" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                        },
                        PkFields = new List<string> { "SerialNum", "AccId", "SubAccId", "ClassType", "UseId" }
                    }
                }
            };

            ViewData["Config"] = config;
            ViewData["TableTitle"] = PageTitle;
        }
    }
}