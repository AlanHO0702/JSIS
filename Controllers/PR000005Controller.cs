using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

/// <summary>
/// PR000005 - 應付票據單 自訂按鈕 API 控制器
/// 對應 Delphi: APRdBillHisDLL.pas / APRdBillHisDLL.dfm
/// 功能: btnC3 票據歷史 - 查詢來源單號的票據沖帳紀錄
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PR000005Controller : ControllerBase
{
    private readonly string _cs;

    public PR000005Controller(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到連線字串");
    }

    /// <summary>
    /// 取得 PR000005.cshtml 的內容 (用於動態載入)
    /// GET /api/PR000005/GetViewContent
    /// </summary>
    [HttpGet("GetViewContent")]
    public async Task<IActionResult> GetViewContent()
    {
        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Pages", "CustomButton", "PR000005.cshtml");

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { ok = false, error = "找不到 PR000005.cshtml 文件" });

            var content = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(content, "text/html");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 取得票據歷史資料 (對應 Delphi: btnGetParamsClick → qryBillHis)
    /// GET /api/PR000005/GetBillHis?paperNum=xxx
    /// </summary>
    [HttpGet("GetBillHis")]
    public async Task<IActionResult> GetBillHis([FromQuery] string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "沒有單號" });

        await using var conn = new SqlConnection(_cs);
        try
        {
            await conn.OpenAsync();

            const string sql = @"
                SELECT t1.PaperNum, t3.BillStatusName, t1.PaperDate
                FROM APRdBillActMain t1 WITH (NOLOCK),
                     APRdBillActSub  t2 WITH (NOLOCK),
                     APRdBillStatus  t3 WITH (NOLOCK)
                WHERE t1.PaperNum    = t2.PaperNum
                  AND t1.Finished   IN (1, 4)
                  AND t2.SourNum     = @PaperNum
                  AND t1.BillStatusId = t3.BillStatusId
                ORDER BY t1.PaperDate";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);

            await using var reader = await cmd.ExecuteReaderAsync();

            var rows = new List<object>();
            while (await reader.ReadAsync())
            {
                rows.Add(new
                {
                    paperNum       = reader["PaperNum"]?.ToString() ?? "",
                    billStatusName = reader["BillStatusName"]?.ToString() ?? "",
                    paperDate      = reader.IsDBNull(reader.GetOrdinal("PaperDate"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("PaperDate"))
                });
            }

            return Ok(new { ok = true, rows });
        }
        catch (Exception ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }
    }
}