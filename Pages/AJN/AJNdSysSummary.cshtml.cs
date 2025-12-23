using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using WebRazor.Models;

namespace PcbErpApi.Pages.AJN
{
    /// <summary>
    /// 系統預設摘要 - 三層級主從式編輯頁面
    /// </summary>
    public class AJNdSysSummaryModel : PageModel
    {
        private readonly ILogger<AJNdSysSummaryModel> _logger;

        public AJNdSysSummaryModel(ILogger<AJNdSysSummaryModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 主表格名稱（系統單據主檔）
        /// </summary>
        public string TableName => "AJNdSysPaperIdBas";

        /// <summary>
        /// 頁面標題
        /// </summary>
        public string PageTitle => "系統預設摘要";

        // 為了相容 View 中的 PaginationModel，提供預設值
        public int PageNumber => 1;
        public int TotalPages => 1;
        public List<CURdTableField> FieldDictList { get; set; } = new List<CURdTableField>();

        public void OnGet()
        {
            // 設定三層級主從式結構
            var config = new MasterMultiDetailConfig
            {
                DomId = "sysSummary",
                MasterTitle = "系統作業",
                MasterTable = "AJNdSysPaperIdBas",
                MasterDict = "AJNdSysPaperIdBas",
                // 不指定 MasterApi，使用預設的 CommonTable API
                MasterTop = 200,

                // ★ 使用垂直堆疊佈局
                Layout = LayoutMode.VerticalStack,
                EnableSplitters = true, // 啟用拖曳器功能
                EnableGridCounts = true, // 啟用表格計數顯示

                // ★ 啟用 Detail Focus 聯動功能（與 Delphi 的 gridSubDetail1Enter 類似）
                EnableDetailFocusCascade = true,

                Details = new List<WebRazor.Models.DetailConfig>
                {
                    // 層級 1：規則主檔（對應 Delphi 的 qryDetail1）
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "摘要規則",
                        DetailTable = "AJNdSysPaperIdSub",
                        DetailDict = "AJNdSysPaperIdSub",
                        // 不指定 DetailApi，使用預設的 CommonTable API
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "SysPaperId", Detail = "SysPaperId" }
                        },
                        PkFields = new List<string> { "SysPaperId", "RuleCode" }
                    },

                    // 層級 2：摘要對應科目（對應 Delphi 的 qrySubDetail1）
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "摘要對應科目",
                        DetailTable = "AJNdSysPaperIdSubDtl",
                        DetailDict = "AJNdSysPaperIdSubDtl",
                        // 不指定 DetailApi，使用預設的 CommonTable API
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "SysPaperId", Detail = "SysPaperId" },
                            new KeyMapMulti { Master = "RuleCode", Detail = "RuleCode" }
                        },
                        PkFields = new List<string> { "SysPaperId", "RuleCode", "Item" }
                    }
                }
            };

            ViewData["Config"] = config;
            ViewData["TableTitle"] = PageTitle;
        }
    }
}