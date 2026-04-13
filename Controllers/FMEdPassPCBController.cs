using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;

namespace PcbErpApi.Controllers;

/// <summary>
/// 單批過帳 (FME00021) API Controller
/// 重構版本 - 簡化流程，正確呼叫 FMEdPassChoiceLotPCB SP
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

    #region 匯入批號 (核心功能)

    /// <summary>
    /// 匯入批號
    /// 流程: 取正式號碼 → 建草稿 FMEdPassMain(Finished=0) → 呼叫 FMEdPassChoiceLotPCB → 回傳資料
    /// 對應 Delphi prcImport 程序
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportLot([FromBody] ImportLotRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.LotNum))
                return BadRequest(new { success = false, message = "請輸入批號" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 1. 決定要使用的 PaperNum
            //    - 若前端已有暫存單號（換批號時），沿用並先清除舊明細
            //    - 若沒有，取正式號碼並建草稿主檔
            string paperNum;
            if (!string.IsNullOrWhiteSpace(request.PaperNum))
            {
                // 確認草稿仍存在
                var exists = await DraftExists(conn, request.PaperNum.Trim());
                if (exists)
                {
                    paperNum = request.PaperNum.Trim();
                    // 清除舊的明細（換批號時重新填入）
                    await CleanDraftDetails(conn, paperNum);
                }
                else
                {
                    // 草稿已逾時被刪除，重新取號
                    paperNum = await CreateDraftPaper(conn, request.UserId ?? "admin");
                    if (string.IsNullOrWhiteSpace(paperNum))
                        return BadRequest(new { success = false, message = "取單號失敗，請重試" });
                }
            }
            else
            {
                // 第一次匯入，取正式號碼並建草稿
                paperNum = await CreateDraftPaper(conn, request.UserId ?? "admin");
                if (string.IsNullOrWhiteSpace(paperNum))
                    return BadRequest(new { success = false, message = "取單號失敗，請重試" });
            }

            // 2. 呼叫 FMEdPassChoiceLotPCB 取得批號資訊
            Dictionary<string, object?>? lotInfo = null;
            var xOutList = new List<Dictionary<string, object?>>();
            var defectList = new List<Dictionary<string, object?>>();

            try
            {
                await using var cmd = new SqlCommand("exec FMEdPassChoiceLotPCB @LotNum, @InStock, @PaperNum", conn);
                cmd.Parameters.AddWithValue("@LotNum", request.LotNum.Trim());
                cmd.Parameters.AddWithValue("@InStock", request.InStock);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);

                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                    lotInfo = ReadRowToDictionary(rd);

                if (await rd.NextResultAsync())
                    while (await rd.ReadAsync())
                        xOutList.Add(ReadRowToDictionary(rd));

                if (await rd.NextResultAsync())
                    while (await rd.ReadAsync())
                        defectList.Add(ReadRowToDictionary(rd));
            }
            catch (SqlException sqlEx)
            {
                // SP 的 RAISERROR（批號暫扣、重複過帳等）直接回傳給前端
                _logger.LogWarning("FMEdPassChoiceLotPCB 回傳錯誤: {Message}", sqlEx.Message);
                return BadRequest(new { success = false, message = sqlEx.Message });
            }

            if (lotInfo == null || lotInfo.Count == 0)
                return BadRequest(new { success = false, message = "無此批號!!" });

            lotInfo["XOutList"] = xOutList;
            lotInfo["DefectList"] = defectList;

            // 3. 補充 Lookup 欄位（製程名稱、階段名稱等）
            await EnrichWithLookupFields(conn, lotInfo);

            // 4. 壓合站：查詢所有層別
            var procCode = GetStringValue(lotInfo, "ProcCode");
            _logger.LogInformation("批號 {LotNum} 的製程代碼: [{ProcCode}]", request.LotNum, procCode);

            if (procCode.Trim() == "P1200")
            {
                var allLayers = await GetAllLayersForLamination(conn, request.LotNum);
                _logger.LogInformation("壓合站查詢到 {Count} 個層別", allLayers.Count);
                if (allLayers.Count > 0)
                {
                    lotInfo["AllLayers"] = allLayers;
                    lotInfo["MinGoodQty"] = allLayers.Min(layer => GetIntValue(layer, "Qnty"));
                }
            }

            // 5. 多報明細
            var xOutDetailList = await GetXOutDetailList(conn, request.LotNum, paperNum);
            if (xOutDetailList.Count > 0)
                lotInfo["XOutDetailList"] = xOutDetailList;

            lotInfo["PaperNum"] = paperNum;
            _logger.LogInformation("匯入批號完成: LotNum={LotNum}, PaperNum={PaperNum}", request.LotNum, paperNum);
            return Ok(new { success = true, lotInfo, paperNum });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入批號失敗: {LotNum}", request.LotNum);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }



    /// <summary>
    /// 補充 Lookup 欄位 (製程名稱、階段名稱、型狀名稱)
    /// 對應 Delphi DFM 中的 TFieldKind = fkLookup 設定
    /// </summary>
    private async Task EnrichWithLookupFields(SqlConnection conn, Dictionary<string, object?> lotInfo)
    {
        _logger.LogInformation("===== EnrichWithLookupFields 開始 =====");
        // 1. 目前製程名稱 (ProcCode → ProcName) 及製程規範參數
        var procCode = GetStringValue(lotInfo, "ProcCode");
        if (!string.IsNullOrEmpty(procCode))
        {
            await using var cmd = new SqlCommand(
                "SELECT ProcName, XOutNeedDefect FROM EMOdProcInfo WITH(NOLOCK) WHERE ProcCode = @Code", conn);
            cmd.Parameters.AddWithValue("@Code", procCode);
            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                var procName = rd["ProcName"]?.ToString();
                if (!string.IsNullOrEmpty(procName))
                    lotInfo["ProcName"] = procName;

                // 取得 XOutNeedDefect (單X需報廢明細)
                if (rd["XOutNeedDefect"] != DBNull.Value)
                    lotInfo["XOutNeedDefect"] = Convert.ToInt32(rd["XOutNeedDefect"]);
            }
        }

        // 2. 下站製程名稱 (AftProc → AftProcName)
        var aftProc = GetStringValue(lotInfo, "AftProc");
        if (!string.IsNullOrEmpty(aftProc))
        {
            var aftProcName = await LookupValue(conn,
                "SELECT ProcName FROM EMOdProcInfo WITH(NOLOCK) WHERE ProcCode = @Code",
                aftProc);
            if (!string.IsNullOrEmpty(aftProcName))
                lotInfo["AftProcName"] = aftProcName;
        }

        // 3. 目前階段名稱 (LayerId → LayerName)
        var layerId = GetStringValue(lotInfo, "LayerId");
        if (!string.IsNullOrEmpty(layerId))
        {
            var layerName = await LookupValue(conn,
                "SELECT LayerName FROM EMOdProdLayer WITH(NOLOCK) WHERE LayerId = @Code",
                layerId);
            if (!string.IsNullOrEmpty(layerName))
                lotInfo["LayerName"] = layerName;
        }

        // 4. 過帳後階段名稱 (AftLayer → AftLayerName)
        var aftLayer = GetStringValue(lotInfo, "AftLayer");
        if (!string.IsNullOrEmpty(aftLayer))
        {
            var aftLayerName = await LookupValue(conn,
                "SELECT LayerName FROM EMOdProdLayer WITH(NOLOCK) WHERE LayerId = @Code",
                aftLayer);
            if (!string.IsNullOrEmpty(aftLayerName))
                lotInfo["AftLayerName"] = aftLayerName;
        }

        // 5. 目前型狀名稱 (POP → POPName)
        var pop = GetIntValue(lotInfo, "POP");
        if (pop > 0)
        {
            var popName = await LookupValue(conn,
                "SELECT POPName FROM EMOdPOP WITH(NOLOCK) WHERE POP = @Code",
                pop);
            if (!string.IsNullOrEmpty(popName))
                lotInfo["POPName"] = popName;
        }

        // 6. 過帳後型狀名稱 (AftPOP → AftPOPName)
        var aftPOP = GetIntValue(lotInfo, "AftPOP");
        if (aftPOP > 0)
        {
            var aftPOPName = await LookupValue(conn,
                "SELECT POPName FROM EMOdPOP WITH(NOLOCK) WHERE POP = @Code",
                aftPOP);
            if (!string.IsNullOrEmpty(aftPOPName))
                lotInfo["AftPOPName"] = aftPOPName;
        }

        // 7. 產品類別 & 允收X報數 & 排版數量 (PartNum → ProdStyle, AllowXOutQnty, 排版數)
        var partNum = GetStringValue(lotInfo, "PartNum");
        if (!string.IsNullOrEmpty(partNum))
        {
            // 7.1 查詢產品類別和允收X報數
            await using (var cmd = new SqlCommand(
                "SELECT ProdStyle, AllowXOutQnty FROM MINdMatInfo WITH(NOLOCK) WHERE PartNum = @Code", conn))
            {
                cmd.Parameters.AddWithValue("@Code", partNum);
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    var prodStyle = rd["ProdStyle"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(prodStyle))
                        lotInfo["ProdStyle"] = prodStyle;

                    if (!rd.IsDBNull(rd.GetOrdinal("AllowXOutQnty")))
                        lotInfo["AllowXOutQnty"] = Convert.ToInt32(rd["AllowXOutQnty"]);
                }
            }

            // 7.2 查詢排版數量 (從 EMOdProdPOP 表查詢 POP=3撈邊 和 POP=4成型)
            var revision = GetStringValue(lotInfo, "Revision");
            try
            {
                _logger.LogInformation("開始查詢排版數: PartNum={PartNum}, Revision={Revision}", partNum, revision);

                await using var cmd = new SqlCommand(
                    @"SELECT POP, AftPiece
                      FROM EMOdProdPOP WITH(NOLOCK)
                      WHERE PartNum = @PartNum AND Revision = @Revision AND POP IN (3, 4)
                      ORDER BY POP", conn);
                cmd.Parameters.AddWithValue("@PartNum", partNum);
                cmd.Parameters.AddWithValue("@Revision", revision ?? "");
                await using var rd = await cmd.ExecuteReaderAsync();

                int? pop3AftPiece = null;
                int? pop4AftPiece = null;
                int rowCount = 0;

                while (await rd.ReadAsync())
                {
                    rowCount++;
                    var popValue = rd["POP"] != DBNull.Value ? Convert.ToInt32(rd["POP"]) : 0;
                    var aftPiece = rd["AftPiece"] != DBNull.Value ? Convert.ToInt32(rd["AftPiece"]) : 0;

                    _logger.LogInformation("找到排版資料: POP={POP}, AftPiece={AftPiece}", popValue, aftPiece);

                    if (popValue == 3)
                        pop3AftPiece = aftPiece;
                    else if (popValue == 4)
                        pop4AftPiece = aftPiece;
                }

                _logger.LogInformation("查詢完成: 共{Count}筆, POP3={POP3}, POP4={POP4}", rowCount, pop3AftPiece, pop4AftPiece);

                // 如果找到兩個值,組合成排版數 (例如 3 x 5)
                if (pop3AftPiece.HasValue && pop4AftPiece.HasValue)
                {
                    lotInfo["XQnty"] = pop3AftPiece.Value;
                    lotInfo["YQnty"] = pop4AftPiece.Value;
                    _logger.LogInformation("排版數設定成功: {X} x {Y}", pop3AftPiece.Value, pop4AftPiece.Value);
                }
                else
                {
                    _logger.LogWarning("排版數不完整: POP3={POP3}, POP4={POP4}", pop3AftPiece, pop4AftPiece);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢排版數量失敗: PartNum={PartNum}, Revision={Revision}", partNum, revision);
            }
        }

        // 8. 途序總站數 (RouteSerial 已由 SP 回傳，這裡補充總站數 TotalRouteSerial)
        //    途序 = RouteSerial / TotalRouteSerial，例如 7 / 14
        var routePartNum = GetStringValue(lotInfo, "PartNum").Trim();
        var routeRevision = GetStringValue(lotInfo, "Revision").Trim();
        var routeLayerId  = GetStringValue(lotInfo, "LayerId").Trim();
        var routeLotNum   = GetStringValue(lotInfo, "LotNum").Trim();

        if (!string.IsNullOrEmpty(routePartNum))
        {
            try
            {
                // 先用 PartNum + Revision + LayerId 查 TmpRouteId，找不到再退回只用 PartNum + Revision
                int total = 0;
                await using var cmd = new SqlCommand(@"
                    DECLARE @TmpId NVARCHAR(20)

                    -- 優先：PartNum + Revision + LayerId 精確查
                    SELECT TOP 1 @TmpId = LTRIM(RTRIM(TmpRouteId))
                    FROM EMOdProdLayer WITH(NOLOCK)
                    WHERE LTRIM(RTRIM(PartNum))  = @PartNum
                      AND LTRIM(RTRIM(Revision)) = @Revision
                      AND LTRIM(RTRIM(LayerId))  = @LayerId
                      AND TmpRouteId IS NOT NULL
                      AND LTRIM(RTRIM(TmpRouteId)) <> ''

                    -- Fallback：只用 PartNum + Revision（取第一筆）
                    IF @TmpId IS NULL OR @TmpId = ''
                        SELECT TOP 1 @TmpId = LTRIM(RTRIM(TmpRouteId))
                        FROM EMOdProdLayer WITH(NOLOCK)
                        WHERE LTRIM(RTRIM(PartNum))  = @PartNum
                          AND LTRIM(RTRIM(Revision)) = @Revision
                          AND TmpRouteId IS NOT NULL
                          AND LTRIM(RTRIM(TmpRouteId)) <> ''

                    -- 計算總站數
                    SELECT COUNT(*) FROM EMOdTmpRouteDtl WITH(NOLOCK) WHERE TmpId = @TmpId", conn);

                cmd.Parameters.AddWithValue("@PartNum",  routePartNum);
                cmd.Parameters.AddWithValue("@Revision", routeRevision);
                cmd.Parameters.AddWithValue("@LayerId",  routeLayerId);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    total = Convert.ToInt32(result);

                if (total > 0)
                    lotInfo["TotalRouteSerial"] = total;

                _logger.LogInformation("途序查詢: LotNum={LotNum}, PartNum={PartNum}, LayerId={LayerId}, Total={Total}",
                    routeLotNum, routePartNum, routeLayerId, total);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "查詢途序總站數失敗: PartNum={PartNum}, LayerId={LayerId}", routePartNum, routeLayerId);
            }
        }
    }

    /// <summary>
    /// 查詢壓合站的所有層別資料
    /// 批號格式: P25050067-06-67 (前綴-批次-層別)
    /// </summary>
    private async Task<List<Dictionary<string, object?>>> GetAllLayersForLamination(SqlConnection conn, string lotNum)
    {
        var layers = new List<Dictionary<string, object?>>();

        try
        {
            _logger.LogInformation("開始查詢壓合站所有層別: {LotNum}", lotNum);

            // 步驟1: 從批號提取 "前綴-批次" (例如 P25050067-06-67 → P25050067-06)
            var lotNumPrefix = "";
            var parts = lotNum.Split('-');

            if (parts.Length >= 2)
            {
                // 取前兩段: P25050067-06
                lotNumPrefix = parts[0] + "-" + parts[1];
            }
            else
            {
                _logger.LogWarning("批號格式不正確，無法提取前綴: {LotNum}", lotNum);
                return layers;
            }

            _logger.LogInformation("批號前綴(含批次): {Prefix}", lotNumPrefix);

            var allLayerLotNums = new List<string>();
            await using (var cmd = new SqlCommand(
                @"SELECT LotNum
                  FROM FMEdV_ProcNIS_ToStd WITH(NOLOCK)
                  WHERE LotNum LIKE @Prefix + '%'
                    AND ProcCode = 'P1200'
                  ORDER BY LotNum", conn))
            {
                cmd.Parameters.AddWithValue("@Prefix", lotNumPrefix);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var layerLotNum = rd["LotNum"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(layerLotNum))
                    {
                        allLayerLotNums.Add(layerLotNum);
                    }
                }
            }

            _logger.LogInformation("找到 {Count} 個壓合站批號: {LotNums}",
                allLayerLotNums.Count, string.Join(", ", allLayerLotNums));

            // 步驟2: 對每個批號呼叫 FMEdPassChoiceLotPCB SP 取得完整資訊
            foreach (var layerLotNum in allLayerLotNums)
            {
                try
                {
                    Dictionary<string, object?>? layer = null;

                    // 先執行 SP 並讀取資料，然後關閉 DataReader
                    await using (var cmd = new SqlCommand("exec FMEdPassChoiceLotPCB @LotNum, @InStock, @PaperNum", conn))
                    {
                        cmd.Parameters.AddWithValue("@LotNum", layerLotNum);
                        cmd.Parameters.AddWithValue("@InStock", 0);
                        cmd.Parameters.AddWithValue("@PaperNum", "");

                        await using var rd = await cmd.ExecuteReaderAsync();
                        if (await rd.ReadAsync())
                        {
                            layer = ReadRowToDictionary(rd);
                        }
                    } // DataReader 在這裡關閉

                    // 確保有資料才繼續處理
                    if (layer != null)
                    {
                        // 補充 Lookup 欄位 (LayerName, ProcName)
                        await EnrichLayerLookupFields(conn, layer);

                        layers.Add(layer);
                        _logger.LogInformation("  層別批號 {LotNum}: Qnty={Qnty}, LayerId={LayerId}",
                            layerLotNum, GetIntValue(layer, "Qnty"), GetStringValue(layer, "LayerId"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "查詢層別批號失敗: {LotNum}", layerLotNum);
                }
            }

            _logger.LogInformation("壓合站查詢完成: 批號 {LotNum} 找到 {Count} 個層別", lotNum, layers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查詢壓合站層別失敗: LotNum={LotNum}", lotNum);
        }

        return layers;
    }

    /// <summary>
    /// 補充層別的 Lookup 欄位 (LayerName, ProcName)
    /// </summary>
    private async Task EnrichLayerLookupFields(SqlConnection conn, Dictionary<string, object?> layer)
    {
        // LayerName
        var layerId = GetStringValue(layer, "LayerId");
        if (!string.IsNullOrEmpty(layerId))
        {
            var layerName = await LookupValue(conn,
                "SELECT LayerName FROM EMOdProdLayer WITH(NOLOCK) WHERE LayerId = @Code",
                layerId);
            if (!string.IsNullOrEmpty(layerName))
                layer["LayerName"] = layerName;
        }

        // ProcName
        var procCode = GetStringValue(layer, "ProcCode");
        if (!string.IsNullOrEmpty(procCode))
        {
            var procName = await LookupValue(conn,
                "SELECT ProcName FROM EMOdProcInfo WITH(NOLOCK) WHERE ProcCode = @Code",
                procCode);
            if (!string.IsNullOrEmpty(procName))
                layer["ProcName"] = procName;
        }
    }

    /// <summary>
    /// 執行 Lookup 查詢
    /// </summary>
    private async Task<string?> LookupValue(SqlConnection conn, string sql, object code)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Code", code);
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString()?.Trim();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 取得多報明細列表
    /// 1. 呼叫 FMEdPassSetXOut SP (將資料寫入 FMEdLotXOutTmp)
    /// 2. 從 FMEdLotXOutTmp 表讀取多報明細
    /// </summary>
    private async Task<List<Dictionary<string, object?>>> GetXOutDetailList(SqlConnection conn, string lotNum, string paperNum)
    {
        var list = new List<Dictionary<string, object?>>();

        try
        {
            // 1. 執行 FMEdPassSetXOut SP (會將資料寫入 FMEdLotXOutTmp)
            await using (var cmd = new SqlCommand("exec FMEdPassSetXOut @LotNum, @PaperNum", conn))
            {
                cmd.Parameters.AddWithValue("@LotNum", lotNum);
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("已執行 FMEdPassSetXOut SP");
            }

            // 2. 從 FMEdLotXOutTmp 表讀取多報明細
            await using (var cmd = new SqlCommand(
                "SELECT * FROM FMEdLotXOutTmp WHERE PaperNum = @PaperNum AND LotNum = @LotNum ORDER BY Item", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                cmd.Parameters.AddWithValue("@LotNum", lotNum);

                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var row = ReadRowToDictionary(rd);
                    list.Add(row);
                }
            }

            _logger.LogInformation("從 FMEdLotXOutTmp 讀取 {Count} 筆多報明細", list.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "取得多報明細失敗: {LotNum}", lotNum);
        }

        return list;
    }

    #endregion

    #region 執行過帳

    /// <summary>
    /// 執行過帳確認
    /// 流程: 驗證草稿存在 → 寫入明細 → 呼叫 FMEdPassResult SP → 更新主檔 Finished=3
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecutePass([FromBody] ExecutePassRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.LotNum))
                return BadRequest(new { success = false, message = "請先匯入批號" });

            if (string.IsNullOrWhiteSpace(request.PaperNum))
                return BadRequest(new { success = false, message = "缺少過帳單號，請重新匯入批號" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 驗證草稿主檔是否仍存在 (Finished=0)
            if (!await DraftExists(conn, request.PaperNum.Trim()))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "過帳單草稿已不存在（可能已逾時或已被清除），請重新匯入批號",
                    needReImport = true
                });
            }

            var paperNum = request.PaperNum.Trim();

            // 讀取系統參數 (對應 PassPCB.pas 各項檢查)
            var procNeedDateCode = await GetSysParamInt(conn, "ProcNeedDateCode");
            var cmpValueAftPass = await GetSysParamInt(conn, "CmpValueAftPass");
            var passUseNHParam = await GetSysParamInt(conn, "PassUseNHParam");
            var passUseTranPQnty = await GetSysParamInt(conn, "PassUseTranPQnty");
            var wipInStk2AssInStk = await GetSysParamInt(conn, "WIPInStk2AssInStk");

            // 檢查 DateCode 是否需要 (對應 PassPCB.pas 第742-750行)
            if (procNeedDateCode == 1)
            {
                int isDateCode = 0;
                await using (var cmd = new SqlCommand(@"
                    SELECT IsDateCode FROM EMOdProcInfo WITH(NOLOCK)
                    WHERE ProcCode = @AftProc", conn))
                {
                    cmd.Parameters.AddWithValue("@AftProc", request.AftProc ?? "");
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                        isDateCode = Convert.ToInt32(result);
                }

                if (isDateCode == 1 && string.IsNullOrWhiteSpace(request.DateCode))
                    return BadRequest(new { success = false, message = "過帳失敗: 請輸入DateCode" });
            }

            await using var tran = conn.BeginTransaction();

            try
            {
                // 1. 更新草稿主檔（確認仍為草稿狀態，補填過帳時間/使用者）
                await using (var cmd = new SqlCommand(@"
                    UPDATE FMEdPassMain
                    SET PaperDate = GETDATE(), UserId = @UserId
                    WHERE PaperNum = @PaperNum AND Finished = 0", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "admin");
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. 建立明細
                await using (var cmd = new SqlCommand(@"
                    INSERT INTO FMEdPassSub (
                        PaperNum, Item, LotNum, PartNum, Revision,
                        ProcCode, LayerId, POP, StockId,
                        AftProc, AftLayer, AftPOP,
                        Qnty, PQnty, SQnty, UQnty, XOutQnty,
                        DateCode, Notes
                    ) VALUES (
                        @PaperNum, 1, @LotNum, @PartNum, @Revision,
                        @ProcCode, @LayerId, @POP, @StockId,
                        @AftProc, @AftLayer, @AftPOP,
                        @Qnty, @PQnty, @SQnty, @UQnty, @XOutQnty,
                        @DateCode, @Notes
                    )", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    cmd.Parameters.AddWithValue("@LotNum", request.LotNum);
                    cmd.Parameters.AddWithValue("@PartNum", request.PartNum ?? "");
                    cmd.Parameters.AddWithValue("@Revision", request.Revision ?? "");
                    cmd.Parameters.AddWithValue("@ProcCode", request.ProcCode ?? "");
                    cmd.Parameters.AddWithValue("@LayerId", request.LayerId ?? "");
                    cmd.Parameters.AddWithValue("@POP", request.POP);
                    cmd.Parameters.AddWithValue("@StockId", request.StockId ?? "");
                    cmd.Parameters.AddWithValue("@AftProc", request.AftProc ?? "");
                    cmd.Parameters.AddWithValue("@AftLayer", request.AftLayer ?? "");
                    cmd.Parameters.AddWithValue("@AftPOP", request.AftPOP);
                    cmd.Parameters.AddWithValue("@Qnty", request.Qnty);
                    cmd.Parameters.AddWithValue("@PQnty", request.PQnty);
                    cmd.Parameters.AddWithValue("@SQnty", request.SQnty);
                    cmd.Parameters.AddWithValue("@UQnty", request.UQnty);
                    cmd.Parameters.AddWithValue("@XOutQnty", request.XOutQnty);
                    cmd.Parameters.AddWithValue("@DateCode", request.DateCode ?? "");
                    cmd.Parameters.AddWithValue("@Notes", request.Notes ?? "");
                    await cmd.ExecuteNonQueryAsync();
                }

                // 5. 處理多報明細 (XOutList)
                if (request.XOutList != null && request.XOutList.Count > 0)
                {
                    foreach (var xOut in request.XOutList)
                    {
                        if (xOut.Qnty > 0)
                        {
                            await using var cmd = new SqlCommand(@"
                                INSERT INTO FMEdPassSubXOut (PaperNum, Item, Qnty, OrgQnty, IsFullScrap)
                                VALUES (@PaperNum, @Item, @Qnty, @OrgQnty, @IsFullScrap)", conn, tran);
                            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                            cmd.Parameters.AddWithValue("@Item", xOut.Item);
                            cmd.Parameters.AddWithValue("@Qnty", xOut.Qnty);
                            cmd.Parameters.AddWithValue("@OrgQnty", xOut.OrgQnty);
                            cmd.Parameters.AddWithValue("@IsFullScrap", xOut.IsFullScrap);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // 6. 處理報廢明細 (DefectList)
                if (request.DefectList != null && request.DefectList.Count > 0)
                {
                    foreach (var defect in request.DefectList)
                    {
                        if (defect.Qnty > 0)
                        {
                            await using var cmd = new SqlCommand(@"
                                INSERT INTO FMEdPassXOutDefect (PaperNum, Item, SerialNum, ClassId, DefectId, DutyProc, Qnty)
                                VALUES (@PaperNum, 1, @SerialNum, @ClassId, @DefectId, @DutyProc, @Qnty)", conn, tran);
                            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                            cmd.Parameters.AddWithValue("@SerialNum", defect.SerialNum);
                            cmd.Parameters.AddWithValue("@ClassId", defect.ClassId ?? "");
                            cmd.Parameters.AddWithValue("@DefectId", defect.DefectId ?? "");
                            cmd.Parameters.AddWithValue("@DutyProc", defect.DutyProc ?? "");
                            cmd.Parameters.AddWithValue("@Qnty", defect.Qnty);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // 7. 檢查下站製程是否為檢驗站 (對應 PassPCB.pas 第757-765行)
                int isCheckPass = 0;
                await using (var cmd = new SqlCommand(@"
                    SELECT IsCheckPass FROM EMOdProcInfo WITH(NOLOCK)
                    WHERE ProcCode = @AftProc", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@AftProc", request.AftProc ?? "");
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                        isCheckPass = Convert.ToInt32(result);
                }
                _logger.LogInformation("過帳 {PaperNum}: 下站製程 {AftProc}, IsCheckPass={IsCheckPass}",
                    paperNum, request.AftProc, isCheckPass);

                // 7.1 多產倉預檢 (對應 PassPCB.pas 第774-784行)
                if (passUseTranPQnty == 1)
                {
                    try
                    {
                        await using var cmd = new SqlCommand("FMEdLotXOutDivPreChk", conn, tran);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                        await cmd.ExecuteNonQueryAsync();
                        _logger.LogInformation("多產倉預檢完成: {PaperNum}", paperNum);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "FMEdLotXOutDivPreChk 執行失敗: {PaperNum}", paperNum);
                        // 如果預檢失敗，可能需要 rollback
                        throw;
                    }
                }

                // 8. 呼叫過帳結果 SP
                await using (var cmd = new SqlCommand("FMEdPassResult", conn, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    await using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync()) { }
                }

                // 9. 更新主檔狀態為完成 (Finished=3, FlowStatus=31 表示已完成)
                await using (var cmd = new SqlCommand(@"
                    UPDATE FMEdPassMain
                    SET Finished = 3, FlowStatus = 31, FinishUser = @UserId, FinishDate = GETDATE()
                    WHERE PaperNum = @PaperNum", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "admin");
                    await cmd.ExecuteNonQueryAsync();
                }

                // 10. 過帳後額外處理 (根據系統參數)

                // 10.1 合併分批自動過帳 (對應 PassPCB.pas 第922-930行)
                // 注意：這個要在計算產能之前執行
                try
                {
                    await using var cmd = new SqlCommand("FMEdMergDivAutoPass", conn, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "admin");
                    await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation("合併分批自動過帳完成: {PaperNum}", paperNum);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "FMEdMergDivAutoPass 執行失敗: {PaperNum}", paperNum);
                    // 這個失敗不影響主流程，繼續執行
                }

                // 10.2 製程參數控管 + 過帳後計算產能 (對應 PassPCB.pas 第933-944行)
                if (passUseNHParam == 1 && cmpValueAftPass == 1)
                {
                    try
                    {
                        await using var cmd = new SqlCommand("FMEdCmpValueAftPass", conn, tran);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                        cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "admin");
                        await cmd.ExecuteNonQueryAsync();
                        _logger.LogInformation("過帳後計算產能完成: {PaperNum}", paperNum);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "FMEdCmpValueAftPass 執行失敗: {PaperNum}", paperNum);
                    }
                }

                await tran.CommitAsync();

                _logger.LogInformation("過帳成功: {PaperNum}, 批號: {LotNum}", paperNum, request.LotNum);
                return Ok(new { success = true, message = $"已產生過帳單號: {paperNum}", paperNum });
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行過帳失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region WIP 現帳查詢 (批號選擇視窗)

    /// <summary>
    /// WIP 現帳查詢 - 呼叫 FMEdPassChoice4LotPass SP
    /// 用於批號選擇彈出視窗
    /// </summary>
    [HttpPost("wip-query")]
    public async Task<IActionResult> QueryWipLots([FromBody] WipQueryRequest request)
    {
        try
        {
            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 建構查詢條件
            // SP 參數: @sCondition, @InStock, @DateCode
            // @sCondition 格式: " and ProcCode='P6300'" 或 " and LotNum like 'L%'"
            var condition = BuildWipCondition(request);

            // Debug: 記錄實際執行的 SQL 和查詢條件
            var sqlDebug = $"exec FMEdPassChoice4LotPass '{condition}', {request.InStock}, '{request.DateCode ?? ""}'";
            _logger.LogInformation("WIP查詢開始");
            _logger.LogInformation("  查詢條件: LotNum={LotNum}, PartNum={PartNum}, Revision={Revision}, ProcCode={ProcCode}, LayerId={LayerId}",
                request.LotNum ?? "(空)", request.PartNum ?? "(空)", request.Revision ?? "(空)",
                request.ProcCode ?? "(空)", request.LayerId ?? "(空)");
            _logger.LogInformation("  產生的 SQL 條件: {Condition}", condition);
            _logger.LogInformation("  完整 SQL: {SQL}", sqlDebug);

            var list = new List<Dictionary<string, object?>>();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            await using (var cmd = new SqlCommand("exec FMEdPassChoice4LotPass @sCondition, @InStock, @DateCode", conn))
            {
                cmd.CommandTimeout = 60; // WIP 查詢可能較慢
                cmd.Parameters.AddWithValue("@sCondition", condition);
                cmd.Parameters.AddWithValue("@InStock", request.InStock);
                cmd.Parameters.AddWithValue("@DateCode", request.DateCode ?? "");

                await using var rd = await cmd.ExecuteReaderAsync();

                while (await rd.ReadAsync())
                {
                    var row = ReadRowToDictionary(rd);
                    list.Add(row);
                }
            }

            sw.Stop();
            _logger.LogInformation("WIP查詢 SP 執行完成，耗時: {Elapsed}ms, 筆數: {Count}", sw.ElapsedMilliseconds, list.Count);

            // 批次補充 Lookup 欄位 (一次查詢所有對照資料)
            sw.Restart();
            await BatchEnrichWithLookupFields(conn, list);
            sw.Stop();
            _logger.LogInformation("WIP查詢 Lookup 完成，耗時: {Elapsed}ms", sw.ElapsedMilliseconds);

            var warningMsg = "";

            return Ok(new {
                success = true,
                data = list,
                total = list.Count,
                sql = sqlDebug,  // 回傳 SQL 供 Debug
                message = warningMsg
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WIP現帳查詢失敗");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("param/import")]
    public async Task<IActionResult> ImportProcParams([FromBody] ProcParamRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PaperNum))
                return BadRequest(new { success = false, message = "缺少 PaperNum" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await using (var cmd = new SqlCommand("exec FMEdPassImportParam @PaperNum, @UserId", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", request.PaperNum.Trim());
                cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "admin");
                await cmd.ExecuteNonQueryAsync();
            }

            var rows = await GetProcParamDetails(conn, request.PaperNum.Trim());
            return Ok(new { success = true, data = rows, total = rows.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯入製程參數失敗: {PaperNum}", request.PaperNum);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("param/clear")]
    public async Task<IActionResult> ClearProcParams([FromBody] ProcParamRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PaperNum))
                return BadRequest(new { success = false, message = "缺少 PaperNum" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await using (var cmd = new SqlCommand("exec FMEdPassClearParam @PaperNum", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", request.PaperNum.Trim());
                await cmd.ExecuteNonQueryAsync();
            }

            var rows = await GetProcParamDetails(conn, request.PaperNum.Trim());
            return Ok(new { success = true, data = rows, total = rows.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除製程參數失敗: {PaperNum}", request.PaperNum);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("xout/sync")]
    public async Task<IActionResult> SyncXOut([FromBody] XOutSyncRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PaperNum) || string.IsNullOrWhiteSpace(request.LotNum))
                return BadRequest(new { success = false, message = "缺少必要參數" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await using (var cmd = new SqlCommand("exec FMEdPassTmp2XOutNewAdd @PaperNum, @LotNum", conn))
            {
                cmd.Parameters.AddWithValue("@PaperNum", request.PaperNum.Trim());
                cmd.Parameters.AddWithValue("@LotNum", request.LotNum.Trim());
                await cmd.ExecuteNonQueryAsync();
            }

            return Ok(new { success = true, message = "多報同步完成" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "多報同步失敗: PaperNum={PaperNum}, LotNum={LotNum}", request.PaperNum, request.LotNum);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除草稿過帳單（前端取消按鈕或 beforeunload 呼叫）
    /// 只刪除 Finished=0 的草稿，已完成的過帳單不受影響
    /// </summary>
    [HttpPost("cancel-draft")]
    public async Task<IActionResult> CancelDraft([FromBody] CancelDraftRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PaperNum))
                return Ok(new { success = true }); // 無單號，視為已清除

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            await DeleteDraft(conn, request.PaperNum.Trim());

            _logger.LogInformation("草稿已刪除: {PaperNum}", request.PaperNum.Trim());
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "刪除草稿失敗: {PaperNum}", request.PaperNum);
            return Ok(new { success = false, message = ex.Message }); // 前端 beforeunload 不需要等回應，統一回 200
        }
    }

    /// <summary>
    /// 建構 WIP 查詢條件
    /// </summary>
    private string BuildWipCondition(WipQueryRequest request)
    {
        var conditions = new List<string>();

        // 批號篩選
        if (!string.IsNullOrWhiteSpace(request.LotNum))
        {
            var lotNum = SqlEscape(request.LotNum.Trim());
            // 支援模糊查詢
            if (request.LotNum.Contains("*") || request.LotNum.Contains("%"))
            {
                var pattern = lotNum.Replace("*", "%");
                conditions.Add($" and LotNum like '{pattern}'");
            }
            else
            {
                conditions.Add($" and LotNum like '{lotNum}%'");
            }
        }

        // 料號篩選
        if (!string.IsNullOrWhiteSpace(request.PartNum))
        {
            var partNum = SqlEscape(request.PartNum.Trim());
            conditions.Add($" and t1.PartNum like '{partNum}%'");
        }

        // 版序篩選
        if (!string.IsNullOrWhiteSpace(request.Revision))
        {
            var revision = SqlEscape(request.Revision.Trim());
            conditions.Add($" and t1.Revision like '{revision}%'");
        }

        // 製程篩選 (支援製程代碼或製程名稱)
        if (!string.IsNullOrWhiteSpace(request.ProcCode))
        {
            var procInput = SqlEscape(request.ProcCode.Trim());
            // 判斷輸入的是中文還是英文
            bool isChinese = System.Text.RegularExpressions.Regex.IsMatch(request.ProcCode, @"[\u4e00-\u9fa5]");

            if (isChinese)
            {
                // 輸入中文，查詢製程名稱 (支援部分匹配)
                conditions.Add($" and exists (select 1 from EMOdProcInfo WITH(NOLOCK) where ProcCode = t1.ProcCode and ProcName like N'%{procInput}%')");
            }
            else
            {
                // 輸入英文，查詢製程代碼 (支援部分匹配)
                conditions.Add($" and t1.ProcCode like '%{procInput}%'");
            }
        }

        // 階段篩選
        if (!string.IsNullOrWhiteSpace(request.LayerId))
        {
            var layerId = SqlEscape(request.LayerId.Trim());
            conditions.Add($" and t1.LayerId = '{layerId}'");
        }

        return string.Join("", conditions);
    }

    /// <summary>
    /// 跳脫 SQL 字串中的單引號，防止 SQL 注入
    /// </summary>
    private string SqlEscape(string input)
    {
        return input.Replace("'", "''");
    }

    // ── 草稿輔助方法 ──────────────────────────────────────────────────────────

    /// <summary>
    /// 取正式號碼並建立草稿主檔 (Finished=0)
    /// </summary>
    private async Task<string> CreateDraftPaper(SqlConnection conn, string userId)
    {
        string sDate = DateTime.Now.ToString("yyMMdd");
        await using (var cmdDate = new SqlCommand("exec CURdGetServerDateTimeStr", conn))
        {
            await using var rd = await cmdDate.ExecuteReaderAsync();
            if (await rd.ReadAsync())
                sDate = rd["sDate"]?.ToString() ?? sDate;
        }

        string? paperNum;
        await using (var cmdNum = new SqlCommand(
            "exec CURdGetPaperNum @Table, @p1, @p2, @Date, @Head, @UseId", conn))
        {
            cmdNum.Parameters.AddWithValue("@Table", "FMEdPassMain");
            cmdNum.Parameters.AddWithValue("@p1", "");
            cmdNum.Parameters.AddWithValue("@p2", "C");
            cmdNum.Parameters.AddWithValue("@Date", sDate);
            cmdNum.Parameters.AddWithValue("@Head", "A");
            cmdNum.Parameters.AddWithValue("@UseId", userId);
            paperNum = (await cmdNum.ExecuteScalarAsync())?.ToString();
        }

        if (string.IsNullOrWhiteSpace(paperNum))
            return string.Empty;

        await using var ins = new SqlCommand(@"
            INSERT INTO FMEdPassMain (PaperNum, PaperDate, UserId, BuildDate, Status, Finished, UseId, FlowStatus)
            VALUES (@PaperNum, GETDATE(), @UserId, GETDATE(), 0, 0, @UserId, 0)", conn);
        ins.Parameters.AddWithValue("@PaperNum", paperNum);
        ins.Parameters.AddWithValue("@UserId", userId);
        await ins.ExecuteNonQueryAsync();

        _logger.LogInformation("建立草稿過帳單: {PaperNum}", paperNum);
        return paperNum;
    }

    /// <summary>
    /// 驗證草稿是否仍存在 (Finished=0)
    /// </summary>
    private static async Task<bool> DraftExists(SqlConnection conn, string paperNum)
    {
        await using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM FMEdPassMain WITH(NOLOCK) WHERE PaperNum=@PaperNum AND Finished=0", conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);
        var cnt = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return cnt > 0;
    }

    /// <summary>
    /// 清除草稿明細（換批號時呼叫），保留主檔
    /// </summary>
    private static async Task CleanDraftDetails(SqlConnection conn, string paperNum)
    {
        foreach (var sql in new[]
        {
            "DELETE FROM FMEdPassSub WHERE PaperNum=@PaperNum",
            "DELETE FROM FMEdPassSubXOut WHERE PaperNum=@PaperNum",
            "DELETE FROM FMEdPassXOutDefect WHERE PaperNum=@PaperNum"
        })
        {
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// 刪除草稿（主檔 + 所有明細），供取消或 beforeunload 呼叫
    /// </summary>
    private static async Task DeleteDraft(SqlConnection conn, string paperNum)
    {
        foreach (var sql in new[]
        {
            "DELETE FROM FMEdPassSubXOut WHERE PaperNum=@PaperNum",
            "DELETE FROM FMEdPassXOutDefect WHERE PaperNum=@PaperNum",
            "DELETE FROM FMEdPassSub WHERE PaperNum=@PaperNum",
            "DELETE FROM FMEdPassMain WHERE PaperNum=@PaperNum AND Finished=0"
        })
        {
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task<List<Dictionary<string, object?>>> GetProcParamDetails(SqlConnection conn, string paperNum)
    {
        var list = new List<Dictionary<string, object?>>();
        await using var cmd = new SqlCommand(
            "SELECT * FROM FMEdPassProcParamDtl WITH(NOLOCK) WHERE PaperNum = @PaperNum", conn);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(ReadRowToDictionary(rd));
        }
        return list;
    }

    #endregion

    #region System Parameter Helper

    /// <summary>
    /// 取得系統參數值
    /// 表結構：SystemId(1), ParamId(2), Notes(3), Value(4), ParamType(5), ...
    /// </summary>
    private async Task<string> GetSysParam(SqlConnection conn, string paramId, SqlTransaction? tran = null)
    {
        try
        {
            var cmd = new SqlCommand(@"
                SELECT Value FROM CURdSysParams WITH(NOLOCK)
                WHERE ParamId = @ParamId", conn, tran);
            cmd.Parameters.AddWithValue("@ParamId", paramId);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                return result.ToString()?.Trim() ?? "0";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "讀取系統參數失敗: {ParamId}", paramId);
        }

        return "0";
    }

    /// <summary>
    /// 取得系統參數整數值
    /// </summary>
    private async Task<int> GetSysParamInt(SqlConnection conn, string paramId, SqlTransaction? tran = null)
    {
        var value = await GetSysParam(conn, paramId, tran);
        return int.TryParse(value, out var result) ? result : 0;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 批次補充 Lookup 欄位 (一次查詢所有對照資料，避免逐筆查詢)
    /// </summary>
    private async Task BatchEnrichWithLookupFields(SqlConnection conn, List<Dictionary<string, object?>> items)
    {
        if (items.Count == 0) return;

        // 1. 收集所有不重複的 Code
        var procCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var layerIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pops = new HashSet<int>();
        var partNums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var poTypes = new HashSet<int>();

        foreach (var item in items)
        {
            var procCode = GetStringValue(item, "ProcCode");
            var aftProc = GetStringValue(item, "AftProc");
            var layerId = GetStringValue(item, "LayerId");
            var aftLayer = GetStringValue(item, "AftLayer");
            var pop = GetIntValue(item, "POP");
            var aftPop = GetIntValue(item, "AftPOP");
            var partNum = GetStringValue(item, "PartNum");
            var poType = GetIntValue(item, "POType");

            if (!string.IsNullOrEmpty(procCode)) procCodes.Add(procCode);
            if (!string.IsNullOrEmpty(aftProc)) procCodes.Add(aftProc);
            if (!string.IsNullOrEmpty(layerId)) layerIds.Add(layerId);
            if (!string.IsNullOrEmpty(aftLayer)) layerIds.Add(aftLayer);
            if (pop > 0) pops.Add(pop);
            if (aftPop > 0) pops.Add(aftPop);
            if (!string.IsNullOrEmpty(partNum)) partNums.Add(partNum);
            if (poType >= 0) poTypes.Add(poType);
        }

        // 2. 批次查詢建立對照表
        var procNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var layerNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var popNameMap = new Dictionary<int, string>();
        var prodStyleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var poTypeNameMap = new Dictionary<int, string>();

        // 查詢製程名稱
        if (procCodes.Count > 0)
        {
            var inClause = string.Join(",", procCodes.Select(c => $"'{c.Replace("'", "''")}'"));
            await using var cmd = new SqlCommand($"SELECT ProcCode, ProcName FROM EMOdProcInfo WITH(NOLOCK) WHERE ProcCode IN ({inClause})", conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var code = rd["ProcCode"]?.ToString()?.Trim() ?? "";
                var name = rd["ProcName"]?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(code)) procNameMap[code] = name;
            }
        }

        // 查詢階段名稱
        if (layerIds.Count > 0)
        {
            var inClause = string.Join(",", layerIds.Select(c => $"'{c.Replace("'", "''")}'"));
            await using var cmd = new SqlCommand($"SELECT LayerId, LayerName FROM EMOdProdLayer WITH(NOLOCK) WHERE LayerId IN ({inClause})", conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var code = rd["LayerId"]?.ToString()?.Trim() ?? "";
                var name = rd["LayerName"]?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(code)) layerNameMap[code] = name;
            }
        }

        // 查詢型狀名稱
        if (pops.Count > 0)
        {
            var inClause = string.Join(",", pops);
            await using var cmd = new SqlCommand($"SELECT POP, POPName FROM EMOdPOP WITH(NOLOCK) WHERE POP IN ({inClause})", conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var pop = Convert.ToInt32(rd["POP"]);
                var name = rd["POPName"]?.ToString()?.Trim() ?? "";
                popNameMap[pop] = name;
            }
        }

        // 查詢產品類別 & 允收X報數 (MINdMatInfo.ProdStyle, AllowXOutQnty)
        var allowXOutQntyMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (partNums.Count > 0)
        {
            var inClause = string.Join(",", partNums.Select(c => $"'{c.Replace("'", "''")}'"));
            await using var cmd = new SqlCommand($"SELECT PartNum, ProdStyle, AllowXOutQnty FROM MINdMatInfo WITH(NOLOCK) WHERE PartNum IN ({inClause})", conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var pn = rd["PartNum"]?.ToString()?.Trim() ?? "";
                var style = rd["ProdStyle"]?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(pn))
                {
                    prodStyleMap[pn] = style;
                    if (!rd.IsDBNull(rd.GetOrdinal("AllowXOutQnty")))
                        allowXOutQntyMap[pn] = Convert.ToInt32(rd["AllowXOutQnty"]);
                }
            }
        }

        // 查詢訂單種類名稱 (FMEdPoType.POTypeName)
        if (poTypes.Count > 0)
        {
            var inClause = string.Join(",", poTypes);
            await using var cmd = new SqlCommand($"SELECT POType, POTypeName FROM FMEdPoType WITH(NOLOCK) WHERE POType IN ({inClause})", conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var poType = Convert.ToInt32(rd["POType"]);
                var name = rd["POTypeName"]?.ToString()?.Trim() ?? "";
                poTypeNameMap[poType] = name;
            }
        }

        // 3. 套用對照表到每筆資料
        foreach (var item in items)
        {
            var procCode = GetStringValue(item, "ProcCode");
            var aftProc = GetStringValue(item, "AftProc");
            var layerId = GetStringValue(item, "LayerId");
            var aftLayer = GetStringValue(item, "AftLayer");
            var pop = GetIntValue(item, "POP");
            var aftPop = GetIntValue(item, "AftPOP");

            if (!string.IsNullOrEmpty(procCode) && procNameMap.TryGetValue(procCode, out var procName))
                item["ProcName"] = procName;

            if (!string.IsNullOrEmpty(aftProc) && procNameMap.TryGetValue(aftProc, out var aftProcName))
                item["AftProcName"] = aftProcName;

            if (!string.IsNullOrEmpty(layerId) && layerNameMap.TryGetValue(layerId, out var layerName))
                item["LayerName"] = layerName;

            if (!string.IsNullOrEmpty(aftLayer) && layerNameMap.TryGetValue(aftLayer, out var aftLayerName))
                item["AftLayerName"] = aftLayerName;

            if (pop > 0 && popNameMap.TryGetValue(pop, out var popName))
                item["POPName"] = popName;

            if (aftPop > 0 && popNameMap.TryGetValue(aftPop, out var aftPopName))
                item["AftPOPName"] = aftPopName;

            // 產品類別 & 允收X報數
            var partNum = GetStringValue(item, "PartNum");
            if (!string.IsNullOrEmpty(partNum))
            {
                if (prodStyleMap.TryGetValue(partNum, out var prodStyle))
                    item["ProdStyle"] = prodStyle;
                if (allowXOutQntyMap.TryGetValue(partNum, out var allowXOutQnty))
                    item["AllowXOutQnty"] = allowXOutQnty;
            }

            // 訂單種類名稱
            var poType = GetIntValue(item, "POType");
            if (poTypeNameMap.TryGetValue(poType, out var poTypeName))
                item["POTypeName"] = poTypeName;
        }
    }

    /// <summary>
    /// 將 DataReader 的一列轉成 Dictionary (忽略大小寫)
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

    private static string GetStringValue(Dictionary<string, object?> dict, string key)
    {
        if (dict.TryGetValue(key, out var val) && val != null)
            return val.ToString()?.Trim() ?? "";
        return "";
    }

    private static int GetIntValue(Dictionary<string, object?> dict, string key)
    {
        if (dict.TryGetValue(key, out var val) && val != null)
        {
            if (int.TryParse(val.ToString(), out var result))
                return result;
        }
        return 0;
    }

    #endregion

    #region Request Models

    public class ImportLotRequest
    {
        /// <summary>批號</summary>
        public string LotNum { get; set; } = "";

        /// <summary>過帳類型: 0=一般, 1=入庫, 255=不限</summary>
        public int InStock { get; set; } = 0;

        /// <summary>單號 (可選，用於檢查重複)</summary>
        public string? PaperNum { get; set; }

        /// <summary>使用者ID</summary>
        public string? UserId { get; set; }
    }

    public class ExecutePassRequest
    {
        // 基本資訊
        public string LotNum { get; set; } = "";
        public string? PaperNum { get; set; }
        public string? UserId { get; set; }

        // 從 lotInfo 帶入的欄位
        public string? PartNum { get; set; }
        public string? Revision { get; set; }
        public string? ProcCode { get; set; }
        public string? LayerId { get; set; }
        public int POP { get; set; }
        public string? StockId { get; set; }
        public string? AftProc { get; set; }
        public string? AftLayer { get; set; }
        public int AftPOP { get; set; }

        // 數量欄位
        public decimal Qnty { get; set; }
        public decimal PQnty { get; set; }
        public decimal SQnty { get; set; }
        public decimal UQnty { get; set; }
        public decimal XOutQnty { get; set; }

        // 其他
        public string? DateCode { get; set; }
        public string? Notes { get; set; }
        public string? Worker { get; set; }
        public string? EquipId { get; set; }

        // 明細資料
        public List<XOutItem>? XOutList { get; set; }
        public List<DefectItem>? DefectList { get; set; }
    }

    public class XOutItem
    {
        public int Item { get; set; }
        public decimal Qnty { get; set; }
        public decimal OrgQnty { get; set; }
        public int IsFullScrap { get; set; }
    }

    public class DefectItem
    {
        public int SerialNum { get; set; }
        public string? ClassId { get; set; }
        public string? DefectId { get; set; }
        public string? DutyProc { get; set; }
        public decimal Qnty { get; set; }
    }

    public class WipQueryRequest
    {
        /// <summary>批號 (支援模糊查詢，如 L2306*)</summary>
        public string? LotNum { get; set; }

        /// <summary>料號</summary>
        public string? PartNum { get; set; }

        /// <summary>版序</summary>
        public string? Revision { get; set; }

        /// <summary>製程代碼</summary>
        public string? ProcCode { get; set; }

        /// <summary>階段</summary>
        public string? LayerId { get; set; }

        /// <summary>過帳類型: 0=一般, 1=入庫, 255=不限</summary>
        public int InStock { get; set; } = 255;

        /// <summary>DateCode 篩選</summary>
        public string? DateCode { get; set; }
    }

    public class ProcParamRequest
    {
        public string PaperNum { get; set; } = "";
        public string? UserId { get; set; }
    }

    public class XOutSyncRequest
    {
        public string PaperNum { get; set; } = "";
        public string LotNum { get; set; } = "";
    }

    public class CancelDraftRequest
    {
        public string? PaperNum { get; set; }
    }

    #endregion
}
