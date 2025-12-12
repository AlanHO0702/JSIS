using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages.AJN
{
    /// <summary>
    /// 部門主檔頁面模型
    /// </summary>
    public class AJNdDepartModel : TableListModel<AjndDepart>
    {
        public AJNdDepartModel(
            IHttpClientFactory httpClientFactory,
            ITableDictionaryService dictService,
            PcbErpContext context,
            ILogger<TableListModel<AjndDepart>> logger)
            : base(httpClientFactory, dictService, context, logger)
        {
        }

        /// <summary>
        /// 對應的資料表名稱
        /// </summary>
        public override string TableName => "AjndDepart";

        /// <summary>
        /// 頁面初始化
        /// </summary>
        public override async Task OnGetAsync()
        {
            // 設定頁面標題
            ViewData["TableTitle"] = "部門主檔";
            ViewData["DictTableName"] = TableName;
            ViewData["AddApiUrl"] = "/api/AJNdDepart";

            // ★ 設定複合主鍵欄位（DepartId + UseId）
            ViewData["KeyFields"] = new[] { "DepartId", "UseId" };

            // 呼叫基底類別的 OnGetAsync
            await base.OnGetAsync();
        }
    }
}
