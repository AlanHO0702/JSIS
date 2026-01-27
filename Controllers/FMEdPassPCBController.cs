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
    /// 匯入批號 - 呼叫 FMEdPassChoiceLotPCB SP 取得批號資訊
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

        // 準備儲存資料的容器
        Dictionary<string, object?>? lotInfo = null;
        var xOutList = new List<Dictionary<string, object?>>();
        var defectList = new List<Dictionary<string, object?>>();

        await using (var cmd = new SqlCommand("exec FMEdPassChoiceLotPCB @LotNum, @InStock, @PaperNum", conn))
        {
            cmd.Parameters.AddWithValue("@LotNum", request.LotNum.Trim());
            cmd.Parameters.AddWithValue("@InStock", request.InStock);
            cmd.Parameters.AddWithValue("@PaperNum", request.PaperNum ?? "");

            try
            {
                await using var rd = await cmd.ExecuteReaderAsync();

                // 1. 讀取第一張表：批號主資訊 (Result Set 1)
                if (await rd.ReadAsync())
                {
                    lotInfo = ReadRowToDictionary(rd);
                }

                // 2. 嘗試讀取第二張表：多報明細 (Result Set 2)
                if (await rd.NextResultAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        xOutList.Add(ReadRowToDictionary(rd));
                    }
                }

                // 3. 嘗試讀取第三張表：報廢明細 (Result Set 3)
                if (await rd.NextResultAsync())
                {
                    while (await rd.ReadAsync())
                    {
                        defectList.Add(ReadRowToDictionary(rd));
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogWarning("FMEdPassChoiceLotPCB 回傳錯誤: {Message}", sqlEx.Message);
                return BadRequest(new { success = false, message = sqlEx.Message });
            }
        }

        if (lotInfo == null || lotInfo.Count == 0)
            return BadRequest(new { success = false, message = "無此批號!!" });

        // 將明細表塞入 lotInfo 中，讓前端可以拿到
        lotInfo["XOutList"] = xOutList;
        lotInfo["DefectList"] = defectList;

        // 補充 Lookup 欄位
        await EnrichWithLookupFields(conn, lotInfo);

        return Ok(new { success = true, lotInfo });
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
        // 1. 目前製程名稱 (ProcCode → ProcName)
        var procCode = GetStringValue(lotInfo, "ProcCode");
        if (!string.IsNullOrEmpty(procCode))
        {
            var procName = await LookupValue(conn,
                "SELECT ProcName FROM EMOdProcInfo WITH(NOLOCK) WHERE ProcCode = @Code",
                procCode);
            if (!string.IsNullOrEmpty(procName))
                lotInfo["ProcName"] = procName;
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

        // 7. 產品類別 & 允收X報數 (PartNum → ProdStyle, AllowXOutQnty from MINdMatInfo)
        var partNum = GetStringValue(lotInfo, "PartNum");
        if (!string.IsNullOrEmpty(partNum))
        {
            await using var cmd = new SqlCommand(
                "SELECT ProdStyle, AllowXOutQnty FROM MINdMatInfo WITH(NOLOCK) WHERE PartNum = @Code", conn);
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

    #endregion

    #region 執行過帳

    /// <summary>
    /// 執行過帳確認
    /// 流程: 建立單號 → 寫入主檔/明細 → 呼叫 FMEdPassResult SP
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecutePass([FromBody] ExecutePassRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.LotNum))
                return BadRequest(new { success = false, message = "請先匯入批號" });

            var cs = _context.Database.GetConnectionString();
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();
            await using var tran = conn.BeginTransaction();

            try
            {
                // 1. 取得伺服器日期
                string sDate = DateTime.Now.ToString("yyMMdd");
                await using (var cmdDate = new SqlCommand("exec CURdGetServerDateTimeStr", conn, tran))
                {
                    await using var rd = await cmdDate.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                        sDate = rd["sDate"]?.ToString() ?? sDate;
                }

                // 2. 取得單號
                string? paperNum = null;
                await using (var cmdNum = new SqlCommand("exec CURdGetPaperNum @Table, @p1, @p2, @Date, @Head, @UseId", conn, tran))
                {
                    cmdNum.Parameters.AddWithValue("@Table", "FMEdPassMain");
                    cmdNum.Parameters.AddWithValue("@p1", "");
                    cmdNum.Parameters.AddWithValue("@p2", "F");
                    cmdNum.Parameters.AddWithValue("@Date", sDate);
                    cmdNum.Parameters.AddWithValue("@Head", "T");
                    cmdNum.Parameters.AddWithValue("@UseId", "FME00021");
                    paperNum = (await cmdNum.ExecuteScalarAsync())?.ToString();
                }

                if (string.IsNullOrWhiteSpace(paperNum))
                {
                    await tran.RollbackAsync();
                    return BadRequest(new { success = false, message = "取單號失敗" });
                }

                // 3. 建立主檔
                await using (var cmd = new SqlCommand(@"
                    INSERT INTO FMEdPassMain (PaperNum, PaperDate, UserId, BuildDate, Status, Finished, UseId, FlowStatus)
                    VALUES (@PaperNum, GETDATE(), @UserId, GETDATE(), 0, 0, 'FME00021', 0)", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "admin");
                    await cmd.ExecuteNonQueryAsync();
                }

                // 4. 建立明細
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

                // 7. 呼叫過帳結果 SP
                string resultMessage = "過帳完成";
                try
                {
                    await using var cmd = new SqlCommand("FMEdPassResult", conn, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    await using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync() && !rd.IsDBNull(0))
                        resultMessage = rd.GetString(0);
                }
                catch (Exception spEx)
                {
                    _logger.LogWarning(spEx, "FMEdPassResult SP 執行失敗");
                }

                // 8. 更新主檔狀態
                await using (var cmd = new SqlCommand(@"
                    UPDATE FMEdPassMain
                    SET Finished = 1, FinishUser = @UserId, FinishDate = GETDATE()
                    WHERE PaperNum = @PaperNum", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PaperNum", paperNum);
                    cmd.Parameters.AddWithValue("@UserId", request.UserId ?? "admin");
                    await cmd.ExecuteNonQueryAsync();
                }

                await tran.CommitAsync();

                _logger.LogInformation("過帳成功: {PaperNum}, 批號: {LotNum}", paperNum, request.LotNum);
                return Ok(new { success = true, message = resultMessage, paperNum });
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
                // 輸入中文，查詢製程名稱
                conditions.Add($" and exists (select 1 from EMOdProcInfo WITH(NOLOCK) where ProcCode = t1.ProcCode and ProcName like N'{procInput}%')");
            }
            else
            {
                // 輸入英文，查詢製程代碼
                conditions.Add($" and t1.ProcCode like '{procInput}%'");
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

    #endregion
}
