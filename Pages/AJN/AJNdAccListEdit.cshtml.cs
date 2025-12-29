using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using WebRazor.Models;

namespace PcbErpApi.Pages.AJN
{
    /// <summary>
    /// 會計科目主檔 - 多層級主從式編輯頁面
    /// </summary>
    public class AJNdAccListEditModel : PageModel
    {
        private readonly ILogger<AJNdAccListEditModel> _logger;

        public AJNdAccListEditModel(ILogger<AJNdAccListEditModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 主表格名稱（總類）
        /// </summary>
        public string TableName => "AJNdAccClass";

        /// <summary>
        /// 頁面標題
        /// </summary>
        public string PageTitle => "會計科目主檔";

        // 為了相容 View 中的 PaginationModel，提供預設值
        public int PageNumber => 1;
        public int TotalPages => 1;
        public List<CURdTableField> FieldDictList { get; set; } = new List<CURdTableField>();

        public void OnGet()
        {
            // 設定多層級主從式結構
            var config = new MasterMultiDetailConfig
            {
                DomId = "accListEdit",
                MasterTitle = "總類",
                MasterTable = "AJNdAccClass",
                MasterDict = "AJNdAccClass",
                MasterTop = 200,
                MasterPkFields = new List<string> { "ClassId", "UseId" },

                // ★ 使用三欄式橫向佈局
                Layout = LayoutMode.ThreeColumn,
                EnableSplitters = true, // 啟用拖曳器功能
                EnableGridCounts = true, // 啟用表格計數顯示

                // ★ 啟用 Detail Focus 聯動功能
                EnableDetailFocusCascade = true,

                Details = new List<WebRazor.Models.DetailConfig>
                {
                    // 層級 1：分類
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "分類",
                        DetailTable = "AJNdAccClassDtl",
                        DetailDict = "AJNdAccClassDtl",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "ClassId", Detail = "ClassId" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                        },
                        PkFields = new List<string> { "ClassDtlId", "UseId" }
                    },

                    // 層級 2：總帳科目
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "總帳科目",
                        DetailTable = "AJNdAccId",
                        DetailDict = "AJNdAccId",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "ClassId", Detail = "ClassId" },
                            new KeyMapMulti { Master = "ClassDtlId", Detail = "ClassDtlId" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                        },
                        PkFields = new List<string> { "AccId", "UseId" }
                    },

                    // 層級 3：明細科目
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "明細科目",
                        DetailTable = "AJNdSubAccId",
                        DetailDict = "AJNdSubAccId",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "AccId", Detail = "AccId" },
                            new KeyMapMulti { Master = "UseId", Detail = "UseId" }
                        },
                        PkFields = new List<string> { "AccId", "SubAccId", "UseId" }
                    }
                }
            };

            ViewData["Config"] = config;
            ViewData["TableTitle"] = PageTitle;
        }
    }
}