using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using WebRazor.Models;

namespace PcbErpApi.Pages.AJN
{
    /// <summary>
    /// 資產負債表設定 - 四層級主從式編輯頁面（BalanceSheet 佈局）
    /// 對應 Delphi 的 BalanceDLL.pas (paperType=1)
    /// </summary>
    public class AJNdBalanceSetModel : PageModel
    {
        private readonly ILogger<AJNdBalanceSetModel> _logger;

        public AJNdBalanceSetModel(ILogger<AJNdBalanceSetModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 頁面標題
        /// </summary>
        public string PageTitle => "資產負債表設定";

        /// <summary>
        /// 主表格名稱（用於相容性）
        /// </summary>
        public string TableName => "AJNdbalanceSet";

        // 為了相容 View 中的 PaginationModel，提供預設值
        public int PageNumber => 1;
        public int TotalPages => 1;
        public List<CURdTableField> FieldDictList { get; set; } = new List<CURdTableField>();

        public void OnGet()
        {
            // 設定三層級主從式結構（BalanceSheet 佈局 - 使用統一的 Master → Details 架構）
            var config = new MasterMultiDetailConfig
            {
                DomId = "balanceSet",

                // ========== Master 設定 ==========
                MasterTitle = "資產負債項目",
                MasterTable = "AJNdbalanceGroup",
                MasterDict = "AJNdbalanceGroup",
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
                    // Detail[0]：資產負債表項目設定（原 Master）
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "資產負債表項目設定",
                        DetailTable = "AJNdbalanceSet",
                        DetailDict = "AJNdbalanceSet",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "SerialNum", Detail = "SerialNum" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                        },
                        PkFields = new List<string> { "Item", "ClassType", "UseId", "SerialNum" }
                    },

                    // Detail[1]：資產負債表科目設定（原 Details[0]）
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "資產負債表科目設定",
                        DetailTable = "AJNdbalanceSetAccId",
                        DetailDict = "AJNdbalanceSetAccId",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "Item", Detail = "Item" },
                            new KeyMapMulti { Master = "ClassType", Detail = "ClassType" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" },
                            new KeyMapMulti { Master = "SerialNum", Detail = "SerialNum" }
                        },
                        PkFields = new List<string> { "Item", "AccId", "SubAccId", "ClassType", "UseId", "SerialNum" }
                    }
                }
            };

            ViewData["Config"] = config;
            ViewData["TableTitle"] = PageTitle;
        }
    }
}
