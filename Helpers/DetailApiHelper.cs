using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PcbErpApi.Helpers;

/// <summary>
/// 單身 API 共用輔助方法
/// </summary>
public static class DetailApiHelper
{
    /// <summary>
    /// 執行刪除操作並處理 SQL 錯誤訊息（含 Trigger 錯誤）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="context">資料庫上下文</param>
    /// <param name="paperNum">單號</param>
    /// <param name="item">項次</param>
    /// <param name="dbSet">要操作的 DbSet</param>
    /// <returns>API 回應結果</returns>
    public static async Task<IActionResult> ExecuteDeleteWithErrorHandling<TEntity>(
        DbContext context,
        string paperNum,
        int item,
        DbSet<TEntity> dbSet)
        where TEntity : class
    {
        try
        {
            // 使用 EF.Property 動態存取屬性，不需要知道具體欄位
            var affected = await dbSet
                .Where(e => EF.Property<string>(e, "PaperNum") == paperNum &&
                           EF.Property<int>(e, "Item") == item)
                .ExecuteDeleteAsync();

            if (affected == 0)
                return new NotFoundResult();

            return new NoContentResult();
        }
        catch (DbUpdateException dbEx)
        {
            // 處理資料庫更新異常（包含 trigger 錯誤）
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

            // 如果是 SQL Server 錯誤，嘗試提取更友善的訊息
            if (innerMessage.Contains("RAISERROR") ||
                innerMessage.Contains("觸發") ||
                innerMessage.Contains("trigger") ||
                innerMessage.Contains("違反"))
            {
                // 提取實際的錯誤訊息（通常在最後一行或特定格式中）
                var lines = innerMessage.Split('\n');
                var errorMsg = lines.FirstOrDefault(l =>
                    !l.Contains("Microsoft.Data.SqlClient") &&
                    !l.Contains("at ") &&
                    !l.Contains("   at") &&
                    !string.IsNullOrWhiteSpace(l))?.Trim() ?? innerMessage;

                return new BadRequestObjectResult(new { error = errorMsg });
            }

            return new ObjectResult(new { error = innerMessage })
            {
                StatusCode = 500
            };
        }
        catch (Exception ex)
        {
            return new ObjectResult(new { error = ex.Message })
            {
                StatusCode = 500
            };
        }
    }

    /// <summary>
    /// 執行刪除操作並處理 SQL 錯誤訊息（簡化版，自動偵測 DbSet）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="context">資料庫上下文</param>
    /// <param name="paperNum">單號</param>
    /// <param name="item">項次</param>
    /// <returns>API 回應結果</returns>
    public static async Task<IActionResult> ExecuteDeleteWithErrorHandling<TEntity>(
        DbContext context,
        string paperNum,
        int item)
        where TEntity : class
    {
        var dbSet = context.Set<TEntity>();
        return await ExecuteDeleteWithErrorHandling(context, paperNum, item, dbSet);
    }
}
