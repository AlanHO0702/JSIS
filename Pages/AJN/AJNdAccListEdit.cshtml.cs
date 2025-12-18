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
                MasterApi = "/api/AJNdAccClass",
                MasterTop = 200,

                // ★ 使用三欄式橫向佈局
                Layout = LayoutMode.ThreeColumn,

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
                        DetailApi = "/api/AJNdAccClassDtl",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "ClassId", Detail = "ClassId" }
                        },
                        PkFields = new List<string> { "ClassId", "ClassDtlId" }
                    },

                    // 層級 2：總帳科目
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "總帳科目",
                        DetailTable = "AJNdAccId",
                        DetailDict = "AJNdAccId",
                        DetailApi = "/api/AJNdAccId",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "ClassId", Detail = "ClassId" },
                            new KeyMapMulti { Master = "ClassDtlId", Detail = "ClassDtlId" }
                        },
                        PkFields = new List<string> { "ClassId", "ClassDtlId", "AccId" }
                    },

                    // 層級 3：明細科目（僅第一層）
                    new WebRazor.Models.DetailConfig
                    {
                        DetailTitle = "明細科目",
                        DetailTable = "AJNdSubAccId",
                        DetailDict = "AJNdSubAccId",
                        DetailApi = "/api/AJNdSubAccId?OnlyFirstLevel=true",
                        KeyMap = new List<KeyMapMulti>
                        {
                            new KeyMapMulti { Master = "AccId", Detail = "AccId" }
                        },
                        PkFields = new List<string> { "AccId", "SubAccId" }
                    }

                    // 註：次明細科目（Detail4）可根據系統參數動態啟用
                }
            };

            ViewData["Config"] = config;
            ViewData["TableTitle"] = PageTitle;
        }
    }
}