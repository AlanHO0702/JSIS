using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PcbErpApi.Data;
using PcbErpApi.Helpers;
using PcbErpApi.Pages.DynamicTemplate;

namespace PcbErpApi.Pages.APR
{
    /// <summary>
    /// APRdCertifMain - 應收憑證單（列表頁）
    /// ✅ 繼承 PaperModel，自動從資料庫載入配置
    /// ✅ 只需要覆寫需要自訂的部分（例如自訂按鈕邏輯）
    /// </summary>
    public class APRdCertifMainModel : PaperModel
    {
        private const string ItemId = "AP000002";

        public APRdCertifMainModel(
            PcbErpContext ctx,
            ITableDictionaryService dictService,
            IHttpClientFactory httpClientFactory)
            : base(ctx, dictService, httpClientFactory)
        {
        }

        /// <summary>
        /// ✅ 覆寫 OnPageHandlerExecuting，在父類方法執行前注入 ItemId
        /// </summary>
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            // 將無參數路由轉換為單參數呼叫 (itemId)
            if (!context.HandlerArguments.ContainsKey("itemId"))
            {
                context.HandlerArguments["itemId"] = ItemId;
            }

            base.OnPageHandlerExecuting(context);
        }

        /// <summary>
        /// ✅ 在頁面處理完成後，設定專屬的 ViewData
        /// </summary>
        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);

            // 🎯 覆寫詳細頁路由模板（父類預設是動態路由，這裡改為固定路由）
            ViewData["SubRouteTemplate"] = "/APR/APRdCertifSub/{PaperNum}";
        }

        // 🎯 以下可以添加應收憑證單列表頁專屬的方法
        // 例如：
        // - OnPostBatchApproveAsync() - 批次審核
        // - OnPostBatchDeleteAsync() - 批次刪除
        // - OnPostExportAsync() - 匯出
        // 等等...
    }
}