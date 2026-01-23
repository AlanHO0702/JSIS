using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;

namespace PcbErpApi.Controllers;

/// <summary>
/// 單批過帳 (FME00021) API Controller
/// 不使用強型別 Model，直接用 Dictionary 動態處理
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class FMEdPassPCBController : ControllerBase
{
    private readonly PcbErpContext _context;
    private readonly ILogger<FMEdPassPCBController> _logger;

    public FMEdPassPCBController(PcbErpContext context, ILogger<FMEdPassPCBController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region 主檔操作

    /// <summary>
    /// 建立新的過帳單據
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreatePaper([FromBody] Dictionary<string, object?>? request)
    {
        try
        {
            var userId = request?.GetValueOrDefault("userId")?.ToString() ?? "admin";
            var useId = request?.GetValueOrDefault("useId")?.ToString() ?? "FME00021";

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 取得伺服器日期時間
            string sDate = DateTime.Now.ToString("yyMMdd");
            string sNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await using (var cmdDate = new SqlCommand("exec CURdGetServerDateTimeStr", conn))
            {
                await using var rd = await cmdDate.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    sDate = rd["sDate"]?.ToString() ?? sDate;
                    sNow = rd["sNow"]?.ToString() ?? sNow;
                }
            }

            // 取得單號
            string? newPaperNum = null;
            var headFirst = "T"; // 過帳單預設前綴

            await using (var cmdNum = new SqlCommand("exec CURdGetPaperNum @p0,@p1,@p2,@p3,@p4,@p5", conn))
            {
                cmdNum.Parameters.AddWithValue("@p0", "FMEdPassMain");
                cmdNum.Parameters.AddWithValue("@p1", "");
                cmdNum.Parameters.AddWithValue("@p2", useId.Length > 0 ? useId.Substring(0, 1) : "");
                cmdNum.Parameters.AddWithValue("@p3", sDate);
                cmdNum.Parameters.AddWithValue("@p4", headFirst);
                cmdNum.Parameters.AddWithValue("@p5", useId);
                var obj = await cmdNum.ExecuteScalarAsync();
                newPaperNum = obj?.ToString();
            }

            if (string.IsNullOrWhiteSpace(newPaperNum))
                return BadRequest(new { success = false, message = "取單號失敗" });

            // 建立主檔
            var insertMas = @"
                INSERT INTO FMEdPassMain (PaperNum, PaperDate, UserId, BuildDate, Status, Finished, UseId, FlowStatus)
                VALUES (@PaperNum, @PaperDate, @UserId, @BuildDate, 0, 0, @UseId, 0)";

            await using (var cmd = new SqlCommand(insertMas, conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                cmd.Parameters.AddWithValue("@PaperDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BuildDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@UseId", useId);
                await cmd.ExecuteNonQueryAsync();
            }

            // 建立空白明細
            var insertSub = @"
                INSERT INTO FMEdPassSub (PaperNum, Item, PQnty, SQnty, UQnty, XOutQnty)
                VALUES (@PaperNum, 1, 0, 0, 0, 0)";

            await using (var cmd = new SqlCommand(insertSub, conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                await cmd.ExecuteNonQueryAsync();
            }

            return Ok(new { success = true, paperNum = newPaperNum });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立過帳單據失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 取得過帳單據完整資料
    /// </summary>
    [HttpGet("{paperNum}")]
    public async Task<IActionResult> GetPaper(string paperNum)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 取得主檔
            Dictionary<string, object?>? master = null;
            await using (var cmd = new SqlCommand("SELECT * FROM FMEdPassMain WITH(NOLOCK) WHERE PaperNum = @PaperNum", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    master = ReadRowToDictionary(rd);
                }
            }

            if (master == null)
                return NotFound(new { message = "找不到單據" });

            // 取得明細
            Dictionary<string, object?>? detail = null;
            await using (var cmd = new SqlCommand("SELECT TOP 1 * FROM FMEdPassSub WITH(NOLOCK) WHERE PaperNum = @PaperNum ORDER BY Item", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    detail = ReadRowToDictionary(rd);
                }
            }

            // 取得多報明細
            var xOutList = new List<Dictionary<string, object?>>();
            await using (var cmd = new SqlCommand("SELECT * FROM FMEdPassSubXOut WITH(NOLOCK) WHERE PaperNum = @PaperNum ORDER BY Item", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    xOutList.Add(ReadRowToDictionary(rd));
                }
            }

            // 取得報廢明細
            var defectList = new List<Dictionary<string, object?>>();
            await using (var cmd = new SqlCommand("SELECT * FROM FMEdPassXOutDefect WITH(NOLOCK) WHERE PaperNum = @PaperNum ORDER BY Item, SerialNum", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    defectList.Add(ReadRowToDictionary(rd));
                }
            }

            return Ok(new
            {
                master,
                detail,
                xOutList,
                defectList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得過帳單據失敗: {PaperNum}", paperNum);
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region 查詢批號

    /// <summary>
    /// 查詢批號資訊 (不建立單據，純查詢)
    /// 使用 FMEdPassChoiceLotPCB SP 取得完整過帳資訊
    /// </summary>
    [HttpPost("lotinfo")]
    public async Task<IActionResult> GetLotInfo([FromBody] Dictionary<string, object?>? request)
    {
        try
        {
            var lotNum = request?.GetValueOrDefault("lotNum")?.ToString();
            var userId = request?.GetValueOrDefault("userId")?.ToString() ?? "";
            var powerType = 0;
            if (request?.GetValueOrDefault("powerType") != null)
                int.TryParse(request["powerType"]?.ToString(), out powerType);

            if (string.IsNullOrEmpty(lotNum))
                return BadRequest(new { success = false, message = "請輸入批號" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 呼叫 SP 查詢批號資訊
            Dictionary<string, object?>? lotInfo = null;

            // 優先使用 FMEdPassChoiceLotPCB SP (Delphi 原始使用的 SP)
            try
            {
                await using var cmd = new SqlCommand("exec FMEdPassChoiceLotPCB @LotNum, @InStock, @PaperNum, @UserId", conn);
                cmd.Parameters.AddWithValue("@LotNum", lotNum);
                cmd.Parameters.AddWithValue("@InStock", powerType);
                cmd.Parameters.AddWithValue("@PaperNum", ""); // 查詢時不帶單號
                cmd.Parameters.AddWithValue("@UserId", userId);

                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    lotInfo = ReadRowToDictionary(rd);
                }
            }
            catch (Exception spEx)
            {
                _logger.LogWarning(spEx, "FMEdPassChoiceLotPCB SP 執行失敗，嘗試 FMEdPassSelectLot");

                // 備援: 嘗試 FMEdPassSelectLot
                try
                {
                    await using var cmd = new SqlCommand("FMEdPassSelectLot", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LotNum", lotNum);
                    cmd.Parameters.AddWithValue("@InStock", powerType);
                    cmd.Parameters.AddWithValue("@PaperNum", "");
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    await using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        lotInfo = ReadRowToDictionary(rd);
                    }
                }
                catch (Exception sp2Ex)
                {
                    _logger.LogWarning(sp2Ex, "FMEdPassSelectLot SP 執行失敗，嘗試簡易查詢");
                    lotInfo = await GetLotInfoSimple(conn, lotNum);
                }
            }

            if (lotInfo == null || lotInfo.Count == 0)
                return Ok(new { success = false, message = "無此批號!!" });

            return Ok(new { success = true, lotInfo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢批號失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 匯入批號資料 (舊版，保留相容性)
    /// 使用 FMEdPassChoiceLotPCB SP 取得完整過帳資訊
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportLot([FromBody] Dictionary<string, object?>? request)
    {
        try
        {
            var paperNum = request?.GetValueOrDefault("paperNum")?.ToString();
            var lotNum = request?.GetValueOrDefault("lotNum")?.ToString();
            var userId = request?.GetValueOrDefault("userId")?.ToString() ?? "";
            var powerType = 0;
            if (request?.GetValueOrDefault("powerType") != null)
                int.TryParse(request["powerType"]?.ToString(), out powerType);

            if (string.IsNullOrEmpty(lotNum))
                return BadRequest(new { success = false, message = "請輸入批號" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 呼叫 SP 查詢批號資訊
            Dictionary<string, object?>? lotInfo = null;

            // 優先使用 FMEdPassChoiceLotPCB SP (Delphi 原始使用的 SP)
            try
            {
                await using var cmd = new SqlCommand("exec FMEdPassChoiceLotPCB @LotNum, @InStock, @PaperNum, @UserId", conn);
                cmd.Parameters.AddWithValue("@LotNum", lotNum);
                cmd.Parameters.AddWithValue("@InStock", powerType);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum ?? "");
                cmd.Parameters.AddWithValue("@UserId", userId);

                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    lotInfo = ReadRowToDictionary(rd);
                }
            }
            catch (Exception spEx)
            {
                _logger.LogWarning(spEx, "FMEdPassChoiceLotPCB SP 執行失敗，嘗試 FMEdPassSelectLot");

                // 備援: 嘗試 FMEdPassSelectLot
                try
                {
                    await using var cmd = new SqlCommand("FMEdPassSelectLot", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LotNum", lotNum);
                    cmd.Parameters.AddWithValue("@InStock", powerType);
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum ?? "");
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    await using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        lotInfo = ReadRowToDictionary(rd);
                    }
                }
                catch (Exception sp2Ex)
                {
                    _logger.LogWarning(sp2Ex, "FMEdPassSelectLot SP 執行失敗，嘗試簡易查詢");
                    // SP 不存在時使用簡易查詢
                    lotInfo = await GetLotInfoSimple(conn, lotNum);
                }
            }

            if (lotInfo == null || lotInfo.Count == 0)
                return BadRequest(new { success = false, message = "無此批號!!" });

            // 更新明細資料
            var updateSql = @"
                UPDATE FMEdPassSub SET
                    LotNum = @LotNum,
                    PartNum = @PartNum,
                    Revision = @Revision,
                    ProcCode = @ProcCode,
                    LayerId = @LayerId,
                    POP = @POP,
                    Qnty = @Qnty,
                    StockId = @StockId,
                    AftProc = @AftProc,
                    AftLayer = @AftLayer,
                    AftPOP = @AftPOP,
                    DateCode = @DateCode,
                    PQnty = @PQnty,
                    SQnty = @SQnty,
                    UQnty = 0,
                    XOutQnty = 0
                WHERE PaperNum = @PaperNum AND Item = 1";

            await using (var cmd = new SqlCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@LotNum", GetValue(lotInfo, "LotNum"));
                cmd.Parameters.AddWithValue("@PartNum", GetValue(lotInfo, "PartNum"));
                cmd.Parameters.AddWithValue("@Revision", GetValue(lotInfo, "Revision"));
                cmd.Parameters.AddWithValue("@ProcCode", GetValue(lotInfo, "ProcCode"));
                cmd.Parameters.AddWithValue("@LayerId", GetValue(lotInfo, "LayerId"));
                cmd.Parameters.AddWithValue("@POP", GetIntValue(lotInfo, "POP"));
                cmd.Parameters.AddWithValue("@Qnty", GetDecimalValue(lotInfo, "RestQnty"));
                cmd.Parameters.AddWithValue("@StockId", GetValue(lotInfo, "StockId"));
                cmd.Parameters.AddWithValue("@AftProc", GetValue(lotInfo, "AftProc"));
                cmd.Parameters.AddWithValue("@AftLayer", GetValue(lotInfo, "AftLayer"));
                cmd.Parameters.AddWithValue("@AftPOP", GetIntValue(lotInfo, "AftPOP"));
                cmd.Parameters.AddWithValue("@DateCode", GetValue(lotInfo, "DateCode"));

                // 過帳數依據 IsShowPQnty 決定
                var isShowPQnty = GetIntValue(lotInfo, "IsShowPQnty");
                var pQnty = isShowPQnty == 1 ? GetDecimalValue(lotInfo, "PQnty") : 0;
                cmd.Parameters.AddWithValue("@PQnty", pQnty);
                cmd.Parameters.AddWithValue("@SQnty", GetDecimalValue(lotInfo, "SQnty"));

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok(new { success = true, lotInfo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入批號失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 簡易查詢批號資訊 - 使用 SELECT * 取得所有欄位
    /// </summary>
    private async Task<Dictionary<string, object?>?> GetLotInfoSimple(SqlConnection conn, string lotNum)
    {
        // 先用 SELECT * 查詢，看有哪些欄位
        var sql = @"SELECT TOP 1 * FROM FMEdLotInfo WITH(NOLOCK) WHERE LotNum = @LotNum";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@LotNum", lotNum);

        try
        {
            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                var dict = ReadRowToDictionary(rd);
                // 加入一些預設值，如果欄位不存在的話
                if (!dict.ContainsKey("IsShowPQnty")) dict["IsShowPQnty"] = 0;
                if (!dict.ContainsKey("IsXOut")) dict["IsXOut"] = 0;
                return dict;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FMEdLotInfo 查詢失敗，嘗試其他資料表");
        }

        // 如果 FMEdLotInfo 不存在，嘗試從其他資料表查詢
        // 可能是 FMEbLot 或其他批號資料表
        var alternateTables = new[] { "FMEbLot", "FMEbLotInfo", "FMEdLot" };
        foreach (var tableName in alternateTables)
        {
            try
            {
                var altSql = $"SELECT TOP 1 * FROM {tableName} WITH(NOLOCK) WHERE LotNum = @LotNum";
                await using var altCmd = new SqlCommand(altSql, conn);
                altCmd.Parameters.AddWithValue("@LotNum", lotNum);
                await using var altRd = await altCmd.ExecuteReaderAsync();
                if (await altRd.ReadAsync())
                {
                    var dict = ReadRowToDictionary(altRd);
                    if (!dict.ContainsKey("IsShowPQnty")) dict["IsShowPQnty"] = 0;
                    if (!dict.ContainsKey("IsXOut")) dict["IsXOut"] = 0;
                    _logger.LogInformation("批號資訊從 {TableName} 查詢成功", tableName);
                    return dict;
                }
            }
            catch
            {
                // 該表不存在，繼續嘗試下一個
            }
        }

        return null;
    }

    #endregion

    #region 明細操作

    /// <summary>
    /// 更新過帳明細
    /// </summary>
    [HttpPut("detail")]
    public async Task<IActionResult> SaveDetail([FromBody] Dictionary<string, object?>? request)
    {
        try
        {
            var paperNum = request?.GetValueOrDefault("paperNum")?.ToString();
            var item = 1;
            if (request?.GetValueOrDefault("item") != null)
                int.TryParse(request["item"]?.ToString(), out item);

            if (string.IsNullOrEmpty(paperNum))
                return BadRequest(new { success = false, message = "缺少單號" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var updateSql = @"
                UPDATE FMEdPassSub SET
                    PQnty = @PQnty,
                    SQnty = @SQnty,
                    UQnty = @UQnty,
                    XOutQnty = @XOutQnty,
                    DateCode = @DateCode,
                    EquipId = @EquipId,
                    Notes = @Notes
                WHERE PaperNum = @PaperNum AND Item = @Item";

            await using var cmd = new SqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            cmd.Parameters.AddWithValue("@Item", item);
            cmd.Parameters.AddWithValue("@PQnty", GetDecimalValue(request, "pQnty"));
            cmd.Parameters.AddWithValue("@SQnty", GetDecimalValue(request, "sQnty"));
            cmd.Parameters.AddWithValue("@UQnty", GetDecimalValue(request, "uQnty"));
            cmd.Parameters.AddWithValue("@XOutQnty", GetDecimalValue(request, "xOutQnty"));
            cmd.Parameters.AddWithValue("@DateCode", GetValue(request, "dateCode"));
            cmd.Parameters.AddWithValue("@EquipId", GetValue(request, "equipId"));
            cmd.Parameters.AddWithValue("@Notes", GetValue(request, "notes"));

            await cmd.ExecuteNonQueryAsync();
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存明細失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region 多報明細

    /// <summary>
    /// 取得多報明細
    /// </summary>
    [HttpGet("{paperNum}/xout")]
    public async Task<IActionResult> GetXOutList(string paperNum)
    {
        var cs = _context.Database.GetConnectionString();
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        var list = new List<Dictionary<string, object?>>();
        await using var cmd = new SqlCommand("SELECT * FROM FMEdPassSubXOut WITH(NOLOCK) WHERE PaperNum = @PaperNum ORDER BY Item", conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(ReadRowToDictionary(rd));
        }

        return Ok(list);
    }

    /// <summary>
    /// 儲存多報明細
    /// </summary>
    [HttpPost("xout")]
    public async Task<IActionResult> SaveXOut([FromBody] Dictionary<string, object?>? request)
    {
        try
        {
            var paperNum = request?.GetValueOrDefault("paperNum")?.ToString();
            var item = GetIntValue(request, "item");

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 檢查是否存在
            var checkSql = "SELECT COUNT(1) FROM FMEdPassSubXOut WHERE PaperNum = @PaperNum AND Item = @Item";
            int exists = 0;
            await using (var cmd = new SqlCommand(checkSql, conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@Item", item);
                exists = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            if (exists > 0)
            {
                var updateSql = "UPDATE FMEdPassSubXOut SET Qnty = @Qnty, OrgQnty = @OrgQnty WHERE PaperNum = @PaperNum AND Item = @Item";
                await using var cmd = new SqlCommand(updateSql, conn);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@Item", item);
                cmd.Parameters.AddWithValue("@Qnty", GetDecimalValue(request, "qnty"));
                cmd.Parameters.AddWithValue("@OrgQnty", GetDecimalValue(request, "orgQnty"));
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                var insertSql = "INSERT INTO FMEdPassSubXOut (PaperNum, Item, Qnty, OrgQnty) VALUES (@PaperNum, @Item, @Qnty, @OrgQnty)";
                await using var cmd = new SqlCommand(insertSql, conn);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@Item", item);
                cmd.Parameters.AddWithValue("@Qnty", GetDecimalValue(request, "qnty"));
                cmd.Parameters.AddWithValue("@OrgQnty", GetDecimalValue(request, "orgQnty"));
                await cmd.ExecuteNonQueryAsync();
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存多報明細失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除多報明細
    /// </summary>
    [HttpDelete("xout/{paperNum}/{item}")]
    public async Task<IActionResult> DeleteXOut(string paperNum, int item)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("DELETE FROM FMEdPassSubXOut WHERE PaperNum = @PaperNum AND Item = @Item", conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            cmd.Parameters.AddWithValue("@Item", item);
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除多報明細失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region 報廢明細

    /// <summary>
    /// 取得報廢明細
    /// </summary>
    [HttpGet("{paperNum}/defect")]
    public async Task<IActionResult> GetDefectList(string paperNum)
    {
        var cs = _context.Database.GetConnectionString();
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        var list = new List<Dictionary<string, object?>>();
        await using var cmd = new SqlCommand("SELECT * FROM FMEdPassXOutDefect WITH(NOLOCK) WHERE PaperNum = @PaperNum ORDER BY Item, SerialNum", conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(ReadRowToDictionary(rd));
        }

        return Ok(list);
    }

    /// <summary>
    /// 儲存報廢明細
    /// </summary>
    [HttpPost("defect")]
    public async Task<IActionResult> SaveDefect([FromBody] Dictionary<string, object?>? request)
    {
        try
        {
            var paperNum = request?.GetValueOrDefault("paperNum")?.ToString();
            var item = GetIntValue(request, "item");
            var serialNum = GetIntValue(request, "serialNum");

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 檢查是否存在
            var checkSql = "SELECT COUNT(1) FROM FMEdPassXOutDefect WHERE PaperNum = @PaperNum AND Item = @Item AND SerialNum = @SerialNum";
            int exists = 0;
            await using (var cmd = new SqlCommand(checkSql, conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@Item", item);
                cmd.Parameters.AddWithValue("@SerialNum", serialNum);
                exists = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            if (exists > 0)
            {
                var updateSql = @"UPDATE FMEdPassXOutDefect SET
                    ClassId = @ClassId, DefectId = @DefectId, DutyProc = @DutyProc, Qnty = @Qnty
                    WHERE PaperNum = @PaperNum AND Item = @Item AND SerialNum = @SerialNum";
                await using var cmd = new SqlCommand(updateSql, conn);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@Item", item);
                cmd.Parameters.AddWithValue("@SerialNum", serialNum);
                cmd.Parameters.AddWithValue("@ClassId", GetValue(request, "classId"));
                cmd.Parameters.AddWithValue("@DefectId", GetValue(request, "defectId"));
                cmd.Parameters.AddWithValue("@DutyProc", GetValue(request, "dutyProc"));
                cmd.Parameters.AddWithValue("@Qnty", GetDecimalValue(request, "qnty"));
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                var insertSql = @"INSERT INTO FMEdPassXOutDefect (PaperNum, Item, SerialNum, ClassId, DefectId, DutyProc, Qnty)
                    VALUES (@PaperNum, @Item, @SerialNum, @ClassId, @DefectId, @DutyProc, @Qnty)";
                await using var cmd = new SqlCommand(insertSql, conn);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@Item", item);
                cmd.Parameters.AddWithValue("@SerialNum", serialNum);
                cmd.Parameters.AddWithValue("@ClassId", GetValue(request, "classId"));
                cmd.Parameters.AddWithValue("@DefectId", GetValue(request, "defectId"));
                cmd.Parameters.AddWithValue("@DutyProc", GetValue(request, "dutyProc"));
                cmd.Parameters.AddWithValue("@Qnty", GetDecimalValue(request, "qnty"));
                await cmd.ExecuteNonQueryAsync();
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存報廢明細失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除報廢明細
    /// </summary>
    [HttpDelete("defect/{paperNum}/{item}/{serialNum}")]
    public async Task<IActionResult> DeleteDefect(string paperNum, int item, int serialNum)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("DELETE FROM FMEdPassXOutDefect WHERE PaperNum = @PaperNum AND Item = @Item AND SerialNum = @SerialNum", conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            cmd.Parameters.AddWithValue("@Item", item);
            cmd.Parameters.AddWithValue("@SerialNum", serialNum);
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除報廢明細失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region 執行過帳

    /// <summary>
    /// 執行過帳確認 (作業型: 自動產生單號 → 寫入資料 → 呼叫 SP)
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecutePass([FromBody] Dictionary<string, object?>? request)
    {
        try
        {
            // 取得畫面傳來的資料
            var lotNum = request?.GetValueOrDefault("lotNum")?.ToString();
            var userId = request?.GetValueOrDefault("userId")?.ToString() ?? "admin";
            var useId = "FME00021";

            if (string.IsNullOrEmpty(lotNum))
                return BadRequest(new { success = false, message = "請先匯入批號" });

            var pQnty = GetDecimalValue(request, "pQnty");
            var sQnty = GetDecimalValue(request, "sQnty");
            var uQnty = GetDecimalValue(request, "uQnty");
            var scrapStrXOutQnty = GetDecimalValue(request, "scrapStrXOutQnty");
            var notes = GetValue(request, "notes")?.ToString() ?? "";

            // 從 lotInfo 帶入的欄位
            var partNum = GetValue(request, "partNum")?.ToString() ?? "";
            var revision = GetValue(request, "revision")?.ToString() ?? "";
            var procCode = GetValue(request, "procCode")?.ToString() ?? "";
            var layerId = GetValue(request, "layerId")?.ToString() ?? "";
            var pop = GetIntValue(request, "pop");
            var aftProc = GetValue(request, "aftProc")?.ToString() ?? "";
            var aftLayer = GetValue(request, "aftLayer")?.ToString() ?? "";
            var aftPop = GetIntValue(request, "aftPop");
            var stockId = GetValue(request, "stockId")?.ToString() ?? "";

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 開始交易
            await using var tran = conn.BeginTransaction();

            try
            {
                // 1. 取得伺服器日期時間
                string sDate = DateTime.Now.ToString("yyMMdd");
                string sNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await using (var cmdDate = new SqlCommand("exec CURdGetServerDateTimeStr", conn, tran))
                {
                    await using var rd = await cmdDate.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        sDate = rd["sDate"]?.ToString() ?? sDate;
                        sNow = rd["sNow"]?.ToString() ?? sNow;
                    }
                }

                // 2. 取得單號
                string? newPaperNum = null;
                var headFirst = "T"; // 過帳單預設前綴

                await using (var cmdNum = new SqlCommand("exec CURdGetPaperNum @p0,@p1,@p2,@p3,@p4,@p5", conn, tran))
                {
                    cmdNum.Parameters.AddWithValue("@p0", "FMEdPassMain");
                    cmdNum.Parameters.AddWithValue("@p1", "");
                    cmdNum.Parameters.AddWithValue("@p2", useId.Length > 0 ? useId.Substring(0, 1) : "");
                    cmdNum.Parameters.AddWithValue("@p3", sDate);
                    cmdNum.Parameters.AddWithValue("@p4", headFirst);
                    cmdNum.Parameters.AddWithValue("@p5", useId);
                    var obj = await cmdNum.ExecuteScalarAsync();
                    newPaperNum = obj?.ToString();
                }

                if (string.IsNullOrWhiteSpace(newPaperNum))
                {
                    await tran.RollbackAsync();
                    return BadRequest(new { success = false, message = "取單號失敗" });
                }

                // 3. 建立主檔
                var insertMas = @"
                    INSERT INTO FMEdPassMain (PaperNum, PaperDate, UserId, BuildDate, Status, Finished, UseId, FlowStatus)
                    VALUES (@PaperNum, @PaperDate, @UserId, @BuildDate, 0, 0, @UseId, 0)";

                await using (var cmd = new SqlCommand(insertMas, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                    cmd.Parameters.AddWithValue("@PaperDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BuildDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UseId", useId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 4. 建立明細 (含批號資訊)
                var insertSub = @"
                    INSERT INTO FMEdPassSub (
                        PaperNum, Item, LotNum, PartNum, Revision, ProcCode, LayerId, POP,
                        AftProc, AftLayer, AftPOP, StockId, PQnty, SQnty, UQnty, XOutQnty, Notes
                    ) VALUES (
                        @PaperNum, 1, @LotNum, @PartNum, @Revision, @ProcCode, @LayerId, @POP,
                        @AftProc, @AftLayer, @AftPOP, @StockId, @PQnty, @SQnty, @UQnty, @XOutQnty, @Notes
                    )";

                await using (var cmd = new SqlCommand(insertSub, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                    cmd.Parameters.AddWithValue("@LotNum", lotNum);
                    cmd.Parameters.AddWithValue("@PartNum", partNum);
                    cmd.Parameters.AddWithValue("@Revision", revision);
                    cmd.Parameters.AddWithValue("@ProcCode", procCode);
                    cmd.Parameters.AddWithValue("@LayerId", layerId);
                    cmd.Parameters.AddWithValue("@POP", pop);
                    cmd.Parameters.AddWithValue("@AftProc", aftProc);
                    cmd.Parameters.AddWithValue("@AftLayer", aftLayer);
                    cmd.Parameters.AddWithValue("@AftPOP", aftPop);
                    cmd.Parameters.AddWithValue("@StockId", stockId);
                    cmd.Parameters.AddWithValue("@PQnty", pQnty);
                    cmd.Parameters.AddWithValue("@SQnty", sQnty);
                    cmd.Parameters.AddWithValue("@UQnty", uQnty);
                    cmd.Parameters.AddWithValue("@XOutQnty", scrapStrXOutQnty);
                    cmd.Parameters.AddWithValue("@Notes", notes);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 5. 處理多報明細 (xOutList)
                var xOutListJson = request?.GetValueOrDefault("xOutList")?.ToString();
                if (!string.IsNullOrEmpty(xOutListJson))
                {
                    try
                    {
                        var xOutList = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(xOutListJson);
                        if (xOutList != null && xOutList.Count > 0)
                        {
                            foreach (var xOut in xOutList)
                            {
                                var xItem = GetIntValue(xOut, "item");
                                var xQnty = GetDecimalValue(xOut, "qnty");
                                var isFullScrap = GetIntValue(xOut, "isFullScrap");

                                if (xQnty > 0)
                                {
                                    var insertXOut = @"
                                        INSERT INTO FMEdPassSubXOut (PaperNum, Item, Qnty, IsFullScrap)
                                        VALUES (@PaperNum, @Item, @Qnty, @IsFullScrap)";
                                    await using var cmd = new SqlCommand(insertXOut, conn, tran);
                                    cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                                    cmd.Parameters.AddWithValue("@Item", xItem);
                                    cmd.Parameters.AddWithValue("@Qnty", xQnty);
                                    cmd.Parameters.AddWithValue("@IsFullScrap", isFullScrap);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "解析 xOutList 失敗");
                    }
                }
                // 也嘗試直接從 JsonElement 解析
                else if (request?.GetValueOrDefault("xOutList") is System.Text.Json.JsonElement xOutElem && xOutElem.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var xOut in xOutElem.EnumerateArray())
                    {
                        var xItem = xOut.TryGetProperty("item", out var itemProp) ? itemProp.GetInt32() : 0;
                        var xQnty = xOut.TryGetProperty("qnty", out var qntyProp) ? qntyProp.GetDecimal() : 0;
                        var isFullScrap = xOut.TryGetProperty("isFullScrap", out var fsProp) ? fsProp.GetInt32() : 0;

                        if (xQnty > 0)
                        {
                            var insertXOut = @"
                                INSERT INTO FMEdPassSubXOut (PaperNum, Item, Qnty, IsFullScrap)
                                VALUES (@PaperNum, @Item, @Qnty, @IsFullScrap)";
                            await using var cmd = new SqlCommand(insertXOut, conn, tran);
                            cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                            cmd.Parameters.AddWithValue("@Item", xItem);
                            cmd.Parameters.AddWithValue("@Qnty", xQnty);
                            cmd.Parameters.AddWithValue("@IsFullScrap", isFullScrap);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // 6. 處理報廢明細 (defectList)
                var defectListJson = request?.GetValueOrDefault("defectList")?.ToString();
                if (!string.IsNullOrEmpty(defectListJson))
                {
                    try
                    {
                        var defectList = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(defectListJson);
                        if (defectList != null && defectList.Count > 0)
                        {
                            foreach (var defect in defectList)
                            {
                                var serialNum = GetIntValue(defect, "serialNum");
                                var classId = GetValue(defect, "classId")?.ToString() ?? "";
                                var defectId = GetValue(defect, "defectId")?.ToString() ?? "";
                                var dutyProc = GetValue(defect, "dutyProc")?.ToString() ?? "";
                                var dQnty = GetDecimalValue(defect, "qnty");

                                if (dQnty > 0)
                                {
                                    var insertDefect = @"
                                        INSERT INTO FMEdPassXOutDefect (PaperNum, Item, SerialNum, ClassId, DefectId, DutyProc, Qnty)
                                        VALUES (@PaperNum, 1, @SerialNum, @ClassId, @DefectId, @DutyProc, @Qnty)";
                                    await using var cmd = new SqlCommand(insertDefect, conn, tran);
                                    cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                                    cmd.Parameters.AddWithValue("@SerialNum", serialNum);
                                    cmd.Parameters.AddWithValue("@ClassId", classId);
                                    cmd.Parameters.AddWithValue("@DefectId", defectId);
                                    cmd.Parameters.AddWithValue("@DutyProc", dutyProc);
                                    cmd.Parameters.AddWithValue("@Qnty", dQnty);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "解析 defectList 失敗");
                    }
                }
                // 也嘗試直接從 JsonElement 解析
                else if (request?.GetValueOrDefault("defectList") is System.Text.Json.JsonElement defectElem && defectElem.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var defect in defectElem.EnumerateArray())
                    {
                        var serialNum = defect.TryGetProperty("serialNum", out var snProp) ? snProp.GetInt32() : 0;
                        var classId = defect.TryGetProperty("classId", out var ciProp) ? ciProp.GetString() ?? "" : "";
                        var defectId = defect.TryGetProperty("defectId", out var diProp) ? diProp.GetString() ?? "" : "";
                        var dutyProc = defect.TryGetProperty("dutyProc", out var dpProp) ? dpProp.GetString() ?? "" : "";
                        var dQnty = defect.TryGetProperty("qnty", out var dqProp) ? dqProp.GetDecimal() : 0;

                        if (dQnty > 0)
                        {
                            var insertDefect = @"
                                INSERT INTO FMEdPassXOutDefect (PaperNum, Item, SerialNum, ClassId, DefectId, DutyProc, Qnty)
                                VALUES (@PaperNum, 1, @SerialNum, @ClassId, @DefectId, @DutyProc, @Qnty)";
                            await using var cmd = new SqlCommand(insertDefect, conn, tran);
                            cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                            cmd.Parameters.AddWithValue("@SerialNum", serialNum);
                            cmd.Parameters.AddWithValue("@ClassId", classId);
                            cmd.Parameters.AddWithValue("@DefectId", defectId);
                            cmd.Parameters.AddWithValue("@DutyProc", dutyProc);
                            cmd.Parameters.AddWithValue("@Qnty", dQnty);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // 7. 呼叫過帳 SP
                string resultMessage = "過帳完成";
                try
                {
                    await using var cmd = new SqlCommand("FMEdPassResult", conn, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);

                    await using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync() && !rd.IsDBNull(0))
                    {
                        resultMessage = rd.GetString(0);
                    }
                }
                catch (Exception spEx)
                {
                    _logger.LogWarning(spEx, "FMEdPassResult SP 執行失敗，繼續更新狀態");
                }

                // 8. 更新主檔狀態
                var updateSql = "UPDATE FMEdPassMain SET Finished = 1, FinishUser = @UserId, FinishDate = GETDATE() WHERE PaperNum = @PaperNum";
                await using (var cmd = new SqlCommand(updateSql, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PaperNum", newPaperNum);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 提交交易
                await tran.CommitAsync();

                return Ok(new { success = true, message = resultMessage, paperNum = newPaperNum });
            }
            catch (Exception innerEx)
            {
                await tran.RollbackAsync();
                throw innerEx;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行過帳失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region 選擇批號列表

    /// <summary>
    /// 取得選擇批號列表
    /// </summary>
    [HttpGet("{paperNum}/selectlot")]
    public async Task<IActionResult> GetSelectLotList(string paperNum)
    {
        var cs = _context.Database.GetConnectionString();
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        var list = new List<Dictionary<string, object?>>();
        var sql = @"SELECT Item, LotNum, LayerId as LayerName, Qnty, PQnty, SQnty, UQnty, DateCode
                    FROM FMEdPassSub WITH(NOLOCK) WHERE PaperNum = @PaperNum ORDER BY Item";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(ReadRowToDictionary(rd));
        }

        return Ok(list);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 將 DataReader 的一列轉成 Dictionary
    /// </summary>
    private static Dictionary<string, object?> ReadRowToDictionary(SqlDataReader reader)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            dict[name] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }
        return dict;
    }

    /// <summary>
    /// 從 Dictionary 取得字串值
    /// </summary>
    private static object GetValue(Dictionary<string, object?>? dict, string key)
    {
        if (dict == null) return DBNull.Value;
        var val = dict.FirstOrDefault(kv => kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;
        return val ?? DBNull.Value;
    }

    /// <summary>
    /// 從 Dictionary 取得整數值
    /// </summary>
    private static int GetIntValue(Dictionary<string, object?>? dict, string key)
    {
        if (dict == null) return 0;
        var val = dict.FirstOrDefault(kv => kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;
        if (val == null) return 0;
        if (int.TryParse(val.ToString(), out var result)) return result;
        return 0;
    }

    /// <summary>
    /// 從 Dictionary 取得 Decimal 值
    /// </summary>
    private static decimal GetDecimalValue(Dictionary<string, object?>? dict, string key)
    {
        if (dict == null) return 0;
        var val = dict.FirstOrDefault(kv => kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;
        if (val == null) return 0;
        if (decimal.TryParse(val.ToString(), out var result)) return result;
        return 0;
    }

    #endregion
}
