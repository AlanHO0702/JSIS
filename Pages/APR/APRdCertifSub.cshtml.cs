using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using PcbErpApi.Data;
using PcbErpApi.Pages.DynamicTemplate;

namespace PcbErpApi.Pages.APR
{
    /// <summary>
    /// APRdCertifSub - 應收憑證單（詳細頁）
    /// ✅ 繼承 PaperDetailModel，自動從資料庫載入配置
    /// ✅ 只需要覆寫需要自訂的部分（例如自訂按鈕邏輯）
    /// </summary>
    public class APRdCertifSubModel : PaperDetailModel
    {
        private const string ItemId = "AP000002";

        public APRdCertifSubModel(
            PcbErpContext ctx,
            ITableDictionaryService dictService,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment env)
            : base(ctx, dictService, httpClientFactory, env)
        {
        }

        /// <summary>
        /// ✅ 覆寫 OnPageHandlerExecuting，在父類方法執行前注入 ItemId
        /// </summary>
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            // 將單一參數路由 {paperNum} 轉換為雙參數呼叫 (itemId, paperNum)
            if (context.HandlerArguments.Count == 1 && context.HandlerArguments.ContainsKey("paperNum"))
            {
                var paperNum = context.HandlerArguments["paperNum"];
                context.HandlerArguments.Clear();
                context.HandlerArguments["itemId"] = ItemId;
                context.HandlerArguments["paperNum"] = paperNum;
            }

            base.OnPageHandlerExecuting(context);
        }

        /// <summary>
        /// ✅ 在頁面處理完成後，設定專屬的 ViewData
        /// </summary>
        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);

            // 🎯 覆寫返回路徑（返回列表頁）
            ViewData["QueryRedirectUrl"] = "/APR/APRdCertifMain";

            // 🎯 覆寫詳細頁路由模板
            ViewData["SubRouteTemplate"] = "/APR/APRdCertifSub/{PaperNum}";
        }

        // 🎯 以下可以添加應收憑證單專屬的方法
        // 例如：
        // - OnPostApproveAsync() - 審核
        // - OnPostRejectAsync() - 退回
        // - OnPostPrintAsync() - 列印
        // 等等...
    }
}