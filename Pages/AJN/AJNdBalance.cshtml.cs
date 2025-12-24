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
            // 設定三層級主從式結構（BalanceSheet 佈局 - 使用統一的 Master → Details 架構）
            var config = new MasterMultiDetailConfig
            {
                DomId = "balance",

                // ========== Master 設定 ==========
                MasterTitle = "損益項目",
                MasterTable = "AJNdIncomeGroup",
                MasterDict = "AJNdIncomeGroup",
                MasterTop = 200,
                MasterPkFields = new List<string> { "SerialNum", "UseId" },

                // ========== Layout 設定 ==========
                Layout = LayoutMode.BalanceSheet,
                MasterHeight = 150, // 控制 Master 區域高度
                EnableDetailFocusCascade = true, // 啟用級聯聯動
                EnableTopToolbar = true, // 啟用「項目新增」工具列
                EnableSplitters = true, // 啟用拖曳器功能
                EnableGridCounts = true, // 啟用表格計數顯示

                // ========== Details 設定 ==========
                Details = new List<WebRazor.Models.DetailConfig>
                {
                    // Detail[0]：損益表項目設定
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "",
                        DetailTable = "AJNdIncome",
                        DetailDict = "AJNdIncome",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "SerialNum", Detail = "SerialNum" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                        },
                        PkFields = new List<string> { "SerialNum", "ClassType", "UseId" }
                    },

                    // Detail[1]：損益表科目設定
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

                    // Detail[2]：匯總排除項目
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "匯總排除項目",
                        DetailTable = "AJNdInComeSumExcep",
                        DetailDict = "AJNdInComeSumExcep",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "SerialNum", Detail = "SerialNum" },
                            new KeyMapMulti { Master = "ClassType", Detail = "ClassType" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                        },
                        PkFields = new List<string> { "SerialNum", "ClassType", "ClassTypeExcep", "UseId" }
                    }
                }
            };

            ViewData["Config"] = config;
            ViewData["TableTitle"] = PageTitle;
        }
    }
}