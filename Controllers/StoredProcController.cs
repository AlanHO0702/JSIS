using System.Data;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StoredProcController : ControllerBase
{
    private readonly string _cs;
    private readonly IConfiguration _cfg;
    private readonly IHttpClientFactory _httpFactory;
    // 白名單：前端 key -> SP 名稱 + 必填/可選參數
    private static readonly Dictionary<string, StoredProcDef> _registry =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["CalcOrderAmount"] = new StoredProcDef(
            ProcName: "dbo.SPOdOrderTotal",            // 有 schema 比較保險
            RequiredParams: new[] { "PaperNum" }
        ),
            // 銷貨單的計算 2025.11.12 james
            ["CalcMPSOutAmount"] = new StoredProcDef(
                ProcName: "dbo.SPOdMPSOutTotal",   // 你的銷貨單總計 SP
                RequiredParams: new[] { "PaperNum" }
            ),
            // ★ 清除單身（三參數版）
            ["ClearOrderDetails"] = new StoredProcDef(
            ProcName: "dbo.SPodClearAllSub",           // 你的 SP 名
            RequiredParams: new[] { "PaperNum", "PaperId", "Item" }
            // 若想把 Item 當可選，就：
            // RequiredParams: new[] { "PaperNum", "PaperId" },
            // OptionalParams: new[] { "Item" }
        ),

            // 新增：製令單清除
            ["FMEdIssueClearLayer"] = new StoredProcDef(
            ProcName: "dbo.FMEdIssueClearLayer",
            // 依你的 SP 參數調整 ↓↓↓
            RequiredParams: new[] { "PaperNum" }
        ),
            // 3.傳票取號
            ["AJNdOCXGetJourId"] = new StoredProcDef(
            ProcName: "dbo.AJNdOCXGetJourId_Plus",           // 你的 SP 名
            RequiredParams: new[] { "PaperId","PaperNum", "HeadParam", "UseId","GetEmpty" }
        ),

            // 製令單重算
            ["FMEdIssueTotalPcsDLL"] = new StoredProcDef(
            ProcName: "dbo.FMEdIssueTotalPcsDLL",
            RequiredParams: new[] { "DLLPaperNum", "iNoReComputeBackQnty" }
        ),

            // 製令單 Lock
            ["FMEdIssueLock"] = new StoredProcDef(
            ProcName: "dbo.FMEdIssueLock",
            RequiredParams: new[] { "PaperNum" }
        ),

            // 製令單解除 Lock
            ["FMEdIssueUnLock"] = new StoredProcDef(
            ProcName: "dbo.FMEdIssueUnLock",
            RequiredParams: new[] { "PaperNum" }
        ),

            // LA201 樣品報價單 - 確認/審核前檢查
            ["SQUdQuotaChkData"] = new StoredProcDef(
            ProcName: "dbo.SQUdQuotaChkData",
            RequiredParams: new[] { "PaperNum" }
        ),

            // LA201 樣品報價單 - 設定規格明細
            ["SQUdGenSetNumTable"] = new StoredProcDef(
            ProcName: "dbo.SQUdGenSetNumTable",
            RequiredParams: new[] { "PaperNum" }
        ),

            // APR00002 確認前檢查
            ["SPOdEInvTypeChk"] = new StoredProcDef(
            ProcName: "dbo.SPOdEInvTypeChk",
            RequiredParams: new[] { "PaperId", "PaperNum", "Action" }
        ),

            // SA000006 電子發票相關 SP
            ["SPOdHintMessage"] = new StoredProcDef(
            ProcName: "dbo.SPOdHintMessage",
            RequiredParams: new[] { "PaperNum", "UserId" }
        ),

            ["SPOdEInvPaperStatusGet"] = new StoredProcDef(
            ProcName: "dbo.SPOdEInvPaperStatusGet",
            RequiredParams: new[] { "PaperId", "PaperNum" },
            OptionalParams: new[] { "EInvStatus" }
        ),

            ["GetPaperStatus"] = new StoredProcDef(
            ProcName: "dbo.GetPaperStatus",
            RequiredParams: new[] { "TableName", "PaperNum" }
        ),

            ["SPOdEInvStatusChk"] = new StoredProcDef(
            ProcName: "dbo.SPOdEInvStatusChk",
            RequiredParams: new[] { "PaperId", "PaperNum" }
        ),

            ["CheckEInvTradeVan"] = new StoredProcDef(
            ProcName: "dbo.CheckEInvTradeVan",
            RequiredParams: new string[] { }
        ),

            ["CheckEInvStatus"] = new StoredProcDef(
            ProcName: "dbo.CheckEInvStatus",
            RequiredParams: new[] { "TableName", "PaperNum" }
        ),

            ["SPOdBackEInvField"] = new StoredProcDef(
            ProcName: "dbo.SPOdBackEInvField",
            RequiredParams: new[] { "PaperId", "PaperNum" }
        ),

            ["SPOdEInvVoidFix"] = new StoredProcDef(
            ProcName: "dbo.SPOdEInvVoidFix",
            RequiredParams: new[] { "Step", "PaperId", "PaperNum", "EInvStatus" }
        ),

        // SA000005, MP000018 折讓單金額檢查
        ["SPOdDebitExamTotalAmount"] = new StoredProcDef(
            ProcName: "dbo.SPOdDebitExamTotalAmount",
            RequiredParams: new[] { "PaperId", "PaperNum", "UserId" }
        ),

            // AA000033 物資轉傳票
            ["AJNdMTL2AAMConfirm"] = new StoredProcDef(
                ProcName: "dbo.AJNdMTL2AAMConfirm",
                RequiredParams: new[] { "HisId", "UseId", "UserId", "Type" }
            ),
            ["AJNdMTL2AAMScrap"] = new StoredProcDef(
                ProcName: "dbo.AJNdMTL2AAMScrap",
                RequiredParams: new[] { "HisId", "UseId", "UserId", "Type", "Scrap" }
            ),

            // AA000016 年度結轉
            ["AJNdTransferAccData"] = new StoredProcDef(
                ProcName: "dbo.AJNdTransferAccData",
                RequiredParams: new[] { "UseId", "FromDate", "DueDate", "SaveType", "UserId" }
            ),

            // BT000011 401營業稅申報
            ["ATXdTaxHistorySet"] = new StoredProcDef(
                ProcName: "dbo.ATXdTaxHistorySet",
                RequiredParams: new[] { "HisId", "UseId", "Is403" }
            ),
            ["ATXdTaxCheck"] = new StoredProcDef(
                ProcName: "dbo.ATXdTaxCheck",
                RequiredParams: new[] { "HisId", "UseId", "UserId", "IsInCond", "IsCurrectCond" }
            ),
            ["ATXdTaxUpdate403"] = new StoredProcDef(
                ProcName: "dbo.ATXdTaxUpdate403",
                RequiredParams: new[] { "HisId", "UseId" }
            )

        };

    public StoredProcController(IConfiguration cfg, PcbErpContext db, IHttpClientFactory httpFactory)
    {
        _cfg = cfg;
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? cfg.GetConnectionString("Default")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到 ConnectionStrings:Default 或 DbContext 連線字串");
        _httpFactory = httpFactory;
    }

    public record ExecSpRequest(string Key, Dictionary<string, object>? Args);
    public record ExecByButtonRequest(string ItemId, string ButtonName, string PaperNum, Dictionary<string, object>? Args);
    public record QueryDirectRequest(string TableName, string? WhereClause, Dictionary<string, object>? Parameters, string[]? Columns);
    public record ReportOrderScheduleRequest(string ItemId, string? ApiKey);
    private record StoredProcDef(string ProcName, string[] RequiredParams, string[]? OptionalParams = null);

    [HttpPost("exec")]
    public async Task<IActionResult> Exec([FromBody] ExecSpRequest req)
    {
        Console.WriteLine($"[StoredProc.Exec] Key={req?.Key}, Args={JsonSerializer.Serialize(req?.Args)}");

        if (string.IsNullOrWhiteSpace(req?.Key) || !_registry.TryGetValue(req.Key, out var def))
            return BadRequest(new { ok = false, error = "未知的作業代碼" });

        var procName = def.ProcName;
        var args = req.Args ?? new();
        foreach (var p in def.RequiredParams)
            if (!args.ContainsKey(p))
                return BadRequest(new { ok = false, error = $"缺少參數: {p}" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();

            // 如果參數中有 PaperId，嘗試解析為 RealTableName
            if (args.TryGetValue("PaperId", out var paperIdObj) && paperIdObj != null)
            {
                var dictPaperId = paperIdObj is JsonElement je ? je.GetString() : paperIdObj.ToString();
                if (!string.IsNullOrWhiteSpace(dictPaperId))
                {
                    var realPaperId = await ResolveRealTableNameAsync(conn, dictPaperId);
                    if (!string.IsNullOrWhiteSpace(realPaperId))
                        args["PaperId"] = realPaperId;
                }
            }

            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await using var cmd = new SqlCommand(procName, conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // 👇 這個 helper 把 JsonElement 轉成 .NET 基本型別
            static object? ToClr(object? v)
            {
                if (v is null) return DBNull.Value;
                if (v is not JsonElement je) return v;

                return je.ValueKind switch
                {
                    JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
                    JsonValueKind.String   => je.GetString(),
                    JsonValueKind.Number   => je.TryGetInt64(out var l) ? l :
                                            je.TryGetDecimal(out var d) ? d :
                                            je.GetDouble(),
                    JsonValueKind.True     => true,
                    JsonValueKind.False    => false,
                    // 參數不會用到 Object/Array；保險：回原字串
                    _ => je.GetRawText()
                };
            }

            // 白名單必填參數
            foreach (var p in def.RequiredParams)
                cmd.Parameters.AddWithValue("@" + p, ToClr(args[p]) ?? DBNull.Value);

            // 白名單可選參數
            if (def.OptionalParams is { Length: > 0 })
                foreach (var p in def.OptionalParams)
                    if (args.TryGetValue(p, out var v))
                        cmd.Parameters.AddWithValue("@" + p, ToClr(v) ?? DBNull.Value);

            var affected = await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
            return Ok(new { ok = true, rowsAffected = affected });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            Console.WriteLine($"[StoredProc.Query][Error] {ex}");
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] ExecSpRequest req)
    {
        Console.WriteLine($"[StoredProc.Query] Key={req?.Key}, Args={JsonSerializer.Serialize(req?.Args)}");

        if (string.IsNullOrWhiteSpace(req?.Key) || !_registry.TryGetValue(req.Key, out var def))
            return BadRequest(new { ok = false, error = "未知的作業代碼" });

        var procName = def.ProcName;
        var args = req.Args ?? new();
        foreach (var p in def.RequiredParams)
            if (!args.ContainsKey(p))
                return BadRequest(new { ok = false, error = $"缺少參數: {p}" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await using var cmd = new SqlCommand(procName, conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // 👇 這個 helper 把 JsonElement 轉成 .NET 基本型別
            static object? ToClr(object? v)
            {
                if (v is null) return DBNull.Value;
                if (v is not JsonElement je) return v;

                return je.ValueKind switch
                {
                    JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
                    JsonValueKind.String   => je.GetString(),
                    JsonValueKind.Number   => je.TryGetInt64(out var l) ? l :
                                            je.TryGetDecimal(out var d) ? d :
                                            je.GetDouble(),
                    JsonValueKind.True     => true,
                    JsonValueKind.False    => false,
                    // 參數不會用到 Object/Array；保險：回原字串
                    _ => je.GetRawText()
                };
            }

            // 白名單必填參數
            foreach (var p in def.RequiredParams)
                cmd.Parameters.AddWithValue("@" + p, ToClr(args[p]) ?? DBNull.Value);

            // 白名單可選參數
            if (def.OptionalParams is { Length: > 0 })
                foreach (var p in def.OptionalParams)
                    if (args.TryGetValue(p, out var v))
                        cmd.Parameters.AddWithValue("@" + p, ToClr(v) ?? DBNull.Value);

            // 使用 ExecuteReaderAsync 讀取所有結果集
            var results = new List<Dictionary<string, object?>>();

            // 使用 using 塊確保 reader 在提交事務前完全釋放
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                do
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object?>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            row[name] = value;
                        }
                        results.Add(row);
                    }
                } while (await reader.NextResultAsync());
            }  // reader 在此處被釋放

            await tx.CommitAsync();
            return Ok(results); // 直接返回結果陣列
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            Console.WriteLine($"[StoredProc.Query][Error] Proc={procName} Args={JsonSerializer.Serialize(args)}");
            Console.WriteLine($"[StoredProc.Query][Error] {ex}");
            if (ex is SqlException sqlEx)
            {
                foreach (SqlError err in sqlEx.Errors)
                {
                    Console.WriteLine($"[StoredProc.Query][SqlError] Number={err.Number} Proc={err.Procedure} Line={err.LineNumber} Message={err.Message}");
                }
            }
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message, proc = procName });
        }
    }

    [HttpPost("execByButton")]
    public async Task<IActionResult> ExecByButton([FromBody] ExecByButtonRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.ItemId) ||
            string.IsNullOrWhiteSpace(req.ButtonName))
        {
            return BadRequest(new { ok = false, error = "ItemId/ButtonName 為必填" });
        }

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            var btn = await LoadButtonAsync(conn, tx, req.ItemId, req.ButtonName);
            if (btn == null)
                return BadRequest(new { ok = false, error = "找不到按鈕設定" });

            if (btn.DesignType != 3)
                return BadRequest(new { ok = false, error = "DesignType 非 3，無法呼叫 SP" });

            var spNameRaw = string.IsNullOrWhiteSpace(btn.ExecSpName) ? btn.SpName : btn.ExecSpName;
            if (string.IsNullOrWhiteSpace(spNameRaw))
                return BadRequest(new { ok = false, error = "找不到 SP 名稱" });

            var spNameNorm = NormalizeIdentifier(spNameRaw)
                .Replace("[", string.Empty)
                .Replace("]", string.Empty);
            var spName = QuoteIdentifier(spNameRaw);
            var opKind = GetOpKind(req.Args);
            var paramDefs = await LoadButtonParamsAsync(conn, tx, req.ItemId, req.ButtonName, opKind);

            var tableMap = await LoadTableMapAsync(conn, tx, req.ItemId);

            if (IsSpName(spNameNorm, "MPHdSendOrderPrice"))
            {
                await EnsureSendOrderSourceFlagsAsync(conn, tx, tableMap, req.PaperNum);
            }

            var systemId = await LoadSystemIdAsync(conn, tx, req.ItemId);
            var userId = User?.Identity?.Name ?? string.Empty;

            if (IsSpName(spNameNorm, "CURdReportOrderRunNow"))
            {
                var reportItemId = await ResolveReportOrderItemIdAsync(
                    conn, tx, tableMap, req.PaperNum, paramDefs, systemId, userId, req.Args);

                if (string.IsNullOrWhiteSpace(reportItemId))
                    return BadRequest(new { ok = false, error = "找不到報表程式項目(ItemId)" });

                var run = await RunReportOrderNowAsync(conn, tx, reportItemId);
                await tx.CommitAsync();
                return Ok(new
                {
                    ok = true,
                    message = "MESGE:已完成",
                    reportItemId,
                    total = run.Total,
                    sent = run.Sent,
                    skipped = run.Skipped
                });
            }

            if (IsSpName(spNameNorm, "CURdReportOrderExam"))
            {
                var reportItemId = await ResolveReportOrderItemIdAsync(
                    conn, tx, tableMap, req.PaperNum, paramDefs, systemId, userId, req.Args);

                if (string.IsNullOrWhiteSpace(reportItemId))
                    return BadRequest(new { ok = false, error = "找不到報表程式項目(ItemId)" });

                var message = await ApproveReportOrderAsync(conn, tx, reportItemId);
                await tx.CommitAsync();
                return Ok(new
                {
                    ok = true,
                    message,
                    reportItemId
                });
            }

            if (IsSpName(spNameNorm, "CURdReportOrderReject"))
            {
                var reportItemId = await ResolveReportOrderItemIdAsync(
                    conn, tx, tableMap, req.PaperNum, paramDefs, systemId, userId, req.Args);

                if (string.IsNullOrWhiteSpace(reportItemId))
                    return BadRequest(new { ok = false, error = "找不到報表程式項目(ItemId)" });

                var message = await RejectReportOrderAsync(conn, tx, reportItemId);
                await tx.CommitAsync();
                return Ok(new
                {
                    ok = true,
                    message,
                    reportItemId
                });
            }

            var placeholders = paramDefs.Count == 0
                ? string.Empty
                : " " + string.Join(", ", Enumerable.Range(1, paramDefs.Count).Select(i => $"@p{i}"));

            await using var cmd = new SqlCommand($"EXEC {spName}{placeholders}", conn, tx)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 120
            };

            var resolvedParams = new List<object>();
            for (var i = 0; i < paramDefs.Count; i++)
            {
                var p = paramDefs[i];
                var value = await ResolveParamValueAsync(conn, tx, tableMap, req.PaperNum, p, systemId, userId, req.Args);
                cmd.Parameters.AddWithValue($"@p{i + 1}", value ?? DBNull.Value);
                resolvedParams.Add(new { paramType = p.ParamType, fieldName = p.ParamFieldName, tableKind = p.TableKind, value = value?.ToString() ?? "(null)" });
            }

            var affected = await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
            return Ok(new { ok = true, rowsAffected = affected, spName = spNameRaw, paramCount = paramDefs.Count, resolvedParams });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    [HttpPost("reportOrder/runNowScheduled")]
    public async Task<IActionResult> RunNowScheduled([FromBody] ReportOrderScheduleRequest req)
    {
        var itemId = (req?.ItemId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(itemId))
            return BadRequest(new { ok = false, error = "ItemId 為必填" });

        var expectedApiKey = (_cfg["ReportOrder:SchedulerApiKey"] ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(expectedApiKey))
        {
            var incomingApiKey = (req?.ApiKey ?? string.Empty).Trim();
            if (!string.Equals(incomingApiKey, expectedApiKey, StringComparison.Ordinal))
                return Unauthorized(new { ok = false, error = "排程驗證失敗" });
        }

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            var run = await RunReportOrderNowAsync(conn, tx, itemId);
            await tx.CommitAsync();

            return Ok(new
            {
                ok = true,
                message = "MESGE:已完成",
                reportItemId = itemId,
                total = run.Total,
                sent = run.Sent,
                skipped = run.Skipped
            });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    private static bool IsSpName(string normalizedName, string targetProc)
    {
        if (string.IsNullOrWhiteSpace(normalizedName) || string.IsNullOrWhiteSpace(targetProc))
            return false;

        return normalizedName.Equals(targetProc, StringComparison.OrdinalIgnoreCase)
            || normalizedName.Equals($"dbo.{targetProc}", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ReportOrderRunResult(int Total, int Sent, int Skipped);
    private sealed record ReportOrderBasRow(int? OrderFreqType, int? DayOfEvery, int? HourOfEvery, int? OrderExpType, int Finished);
    private sealed record ReportOrderScheduleDef(int FreqType, int FreqInterval, int FreqRecurrenceFactor, int StartDate, int StartTime);
    private sealed record ReportOrderCheckRow(string ItemId, int OrderExpType, string PrintRptName, string ReportPath);
    private sealed record ReportOrderSqlRow(string SqlText, string ObjectName, string ItemName);

    private async Task<string?> ResolveReportOrderItemIdAsync(
        SqlConnection conn,
        SqlTransaction tx,
        Dictionary<string, string> tableMap,
        string paperNum,
        List<ButtonParamRow> paramDefs,
        string? systemId,
        string userId,
        Dictionary<string, object>? args)
    {
        var ordered = paramDefs.OrderBy(p => p.SeqNum).ToList();
        foreach (var p in ordered)
        {
            var val = await ResolveParamValueAsync(conn, tx, tableMap, paperNum, p, systemId, userId, args);
            var s = Convert.ToString(val)?.Trim();
            if (!string.IsNullOrWhiteSpace(s)) return s;
        }

        var row = GetArgValue(args ?? new Dictionary<string, object>(), "masterRow");
        var fromMaster = row == null ? null : ReadFieldFromRow(row, "ItemId");
        var fromMasterText = Convert.ToString(fromMaster)?.Trim();
        if (!string.IsNullOrWhiteSpace(fromMasterText)) return fromMasterText;

        var fromPaperNum = (paperNum ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(fromPaperNum) ? null : fromPaperNum;
    }

    private async Task<ReportOrderRunResult> RunReportOrderNowAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        if (!await ReportOrderItemExistsAsync(conn, tx, itemId))
            throw new InvalidOperationException("報表程式項目不存在");

        if (!await ReportOrderItemFinishedAsync(conn, tx, itemId))
            throw new InvalidOperationException("報表程式項目尚未審核");

        var rows = await LoadReportOrderCheckRowsAsync(conn, tx, itemId);
        var sent = 0;
        var skipped = 0;

        foreach (var row in rows)
        {
            var sqlInfo = await LoadReportOrderSqlAsync(conn, tx, row.ItemId);
            if (string.IsNullOrWhiteSpace(sqlInfo.SqlText))
            {
                skipped++;
                continue;
            }

            var attFile = await BuildReportOrderAttachmentAsync(conn, tx, row, sqlInfo);
            if (string.IsNullOrWhiteSpace(attFile))
            {
                skipped++;
                continue;
            }

            await SendReportOrderMailAsync(conn, tx, row.ItemId, attFile);
            sent++;
        }

        return new ReportOrderRunResult(rows.Count, sent, skipped);
    }

    private async Task<string> ApproveReportOrderAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        var bas = await LoadReportOrderBasAsync(conn, tx, itemId)
            ?? throw new InvalidOperationException("報表程式項目不存在");

        if (bas.Finished == 1)
            throw new InvalidOperationException("此程式項目已審核，不可重覆操作");

        await ValidateReportOrderApproveAsync(conn, tx, itemId, bas);

        var scheduleDef = BuildReportOrderScheduleDef(bas);
        var jobName = BuildReportOrderJobName(conn.Database, itemId);
        await DeleteSqlAgentJobIfExistsAsync(conn, tx, jobName);

        var cmdExec = BuildReportOrderScheduleCommand(itemId);
        await CreateSqlAgentJobAsync(conn, tx, jobName, conn.Database, scheduleDef, cmdExec);

        await SetReportOrderFinishedAsync(conn, tx, itemId, 1);
        return "MESGE:已建立排程";
    }

    private async Task<string> RejectReportOrderAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        if (!await ReportOrderItemExistsAsync(conn, tx, itemId))
            throw new InvalidOperationException("報表程式項目不存在");

        var jobName = BuildReportOrderJobName(conn.Database, itemId);
        await DeleteSqlAgentJobIfExistsAsync(conn, tx, jobName);
        await SetReportOrderFinishedAsync(conn, tx, itemId, 0);

        return "MESGE:已退審";
    }

    private static async Task<ReportOrderBasRow?> LoadReportOrderBasAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = @"
SELECT TOP 1
       OrderFreqType,
       DayOfEvery,
       HourOfEvery,
       OrderExpType,
       ISNULL(Finished, 0) AS Finished
  FROM CURdReportOrderBas WITH (NOLOCK)
 WHERE ItemId = @itemId;";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return null;

        return new ReportOrderBasRow(
            OrderFreqType: ReadNullableInt(rd, "OrderFreqType"),
            DayOfEvery: ReadNullableInt(rd, "DayOfEvery"),
            HourOfEvery: ReadNullableInt(rd, "HourOfEvery"),
            OrderExpType: ReadNullableInt(rd, "OrderExpType"),
            Finished: ReadInt(rd, "Finished")
        );
    }

    private static async Task SetReportOrderFinishedAsync(SqlConnection conn, SqlTransaction tx, string itemId, int finished)
    {
        const string sql = "UPDATE CURdReportOrderBas SET Finished = @finished WHERE ItemId = @itemId;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@finished", finished);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var affected = await cmd.ExecuteNonQueryAsync();
        if (affected <= 0)
            throw new InvalidOperationException("更新狀態...發生錯誤");
    }

    private static async Task ValidateReportOrderApproveAsync(
        SqlConnection conn,
        SqlTransaction tx,
        string itemId,
        ReportOrderBasRow bas)
    {
        if (!await ReportOrderHasSubscriberAsync(conn, tx, itemId))
            throw new InvalidOperationException("沒有訂閱者，不可審核");

        var noEmailUser = await FirstSubscriberWithoutEmailAsync(conn, tx, itemId);
        if (!string.IsNullOrWhiteSpace(noEmailUser))
            throw new InvalidOperationException($"訂閱者{noEmailUser}沒有E_Mail資料");

        var disabledUser = await FirstDisabledSubscriberAsync(conn, tx, itemId);
        if (!string.IsNullOrWhiteSpace(disabledUser))
            throw new InvalidOperationException($"訂閱者{disabledUser}已停用");

        if (!bas.OrderFreqType.HasValue || !await ReportOrderFreqTypeExistsAsync(conn, tx, bas.OrderFreqType.Value))
            throw new InvalidOperationException("訂閱頻率不正確");

        if (!bas.OrderExpType.HasValue || !await ReportOrderExpTypeExistsAsync(conn, tx, bas.OrderExpType.Value))
            throw new InvalidOperationException("夾檔種類不正確");

        if (!bas.HourOfEvery.HasValue)
            throw new InvalidOperationException("必須輸入「幾點發送」");

        if (bas.HourOfEvery.Value < 0 || bas.HourOfEvery.Value > 23)
            throw new InvalidOperationException("輸入的「幾點發送」有誤");

        if (bas.OrderFreqType is 1 or 2)
        {
            if (!bas.DayOfEvery.HasValue)
                throw new InvalidOperationException("必須輸入「第幾日發送」");

            if (bas.OrderFreqType == 1 && (bas.DayOfEvery.Value < 1 || bas.DayOfEvery.Value > 31))
                throw new InvalidOperationException("輸入的「第幾日發送」有誤");

            if (bas.OrderFreqType == 2 && (bas.DayOfEvery.Value < 1 || bas.DayOfEvery.Value > 7))
                throw new InvalidOperationException("輸入的「第幾日發送」有誤");
        }
    }

    private static async Task<bool> ReportOrderHasSubscriberAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = "SELECT TOP 1 1 FROM CURdReportOrderUser WITH (NOLOCK) WHERE ItemId = @itemId;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }

    private static async Task<string?> FirstSubscriberWithoutEmailAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = @"
SELECT TOP 1 t1.UserId
  FROM CURdUsers t1 WITH (NOLOCK)
  JOIN CURdReportOrderUser t2 WITH (NOLOCK) ON t1.UserId = t2.UserId
 WHERE t2.ItemId = @itemId
   AND LTRIM(RTRIM(ISNULL(t1.E_Mail, ''))) = '';";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : Convert.ToString(obj);
    }

    private static async Task<string?> FirstDisabledSubscriberAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = @"
SELECT TOP 1 t1.UserId
  FROM CURdUsers t1 WITH (NOLOCK)
  JOIN CURdReportOrderUser t2 WITH (NOLOCK) ON t1.UserId = t2.UserId
 WHERE t2.ItemId = @itemId
   AND ISNULL(t1.Permit, 0) = 0;";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : Convert.ToString(obj);
    }

    private static async Task<bool> ReportOrderFreqTypeExistsAsync(SqlConnection conn, SqlTransaction tx, int orderFreqType)
    {
        const string sql = "SELECT TOP 1 1 FROM CURdReportOrderFreq WITH (NOLOCK) WHERE OrderFreqType = @val;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@val", orderFreqType);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }

    private static async Task<bool> ReportOrderExpTypeExistsAsync(SqlConnection conn, SqlTransaction tx, int orderExpType)
    {
        const string sql = "SELECT TOP 1 1 FROM CURdReportOrderExpType WITH (NOLOCK) WHERE OrderExpType = @val;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@val", orderExpType);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }

    private static ReportOrderScheduleDef BuildReportOrderScheduleDef(ReportOrderBasRow bas)
    {
        var orderFreqType = bas.OrderFreqType ?? 0;
        var dayOfEvery = bas.DayOfEvery ?? 0;
        var hourOfEvery = bas.HourOfEvery ?? 0;

        var startDate = int.Parse(DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        var startTime = hourOfEvery * 10000;

        if (orderFreqType == 1)
            return new ReportOrderScheduleDef(16, dayOfEvery, 1, startDate, startTime);

        if (orderFreqType == 2)
        {
            var weeklyInterval = dayOfEvery switch
            {
                1 => 2,   // Monday
                2 => 4,   // Tuesday
                3 => 8,   // Wednesday
                4 => 16,  // Thursday
                5 => 32,  // Friday
                6 => 64,  // Saturday
                7 => 1,   // Sunday
                _ => 0
            };
            if (weeklyInterval == 0)
                throw new InvalidOperationException("輸入的「第幾日發送」有誤");
            return new ReportOrderScheduleDef(8, weeklyInterval, 1, startDate, startTime);
        }

        if (orderFreqType == 3)
            return new ReportOrderScheduleDef(4, 1, 0, startDate, startTime);

        throw new InvalidOperationException("訂閱頻率不正確");
    }

    private static string BuildReportOrderJobName(string dbName, string itemId)
    {
        var safeDb = string.IsNullOrWhiteSpace(dbName) ? "DB" : dbName.Trim();
        var safeItem = string.IsNullOrWhiteSpace(itemId) ? "Item" : itemId.Trim();
        return $"RptOrder_{safeDb}_{safeItem}";
    }

    private static async Task DeleteSqlAgentJobIfExistsAsync(SqlConnection conn, SqlTransaction tx, string jobName)
    {
        const string checkSql = "SELECT TOP 1 1 FROM msdb.dbo.sysjobs_view WITH (NOLOCK) WHERE name = @jobName;";
        await using (var checkCmd = new SqlCommand(checkSql, conn, tx))
        {
            checkCmd.Parameters.AddWithValue("@jobName", jobName);
            var exists = await checkCmd.ExecuteScalarAsync();
            if (exists == null || exists == DBNull.Value) return;
        }

        const string deleteSql = "EXEC msdb.dbo.sp_delete_job @job_name = @jobName, @delete_unused_schedule = 1;";
        await using var deleteCmd = new SqlCommand(deleteSql, conn, tx);
        deleteCmd.Parameters.AddWithValue("@jobName", jobName);
        await deleteCmd.ExecuteNonQueryAsync();
    }

    private static async Task CreateSqlAgentJobAsync(
        SqlConnection conn,
        SqlTransaction tx,
        string jobName,
        string dbName,
        ReportOrderScheduleDef schedule,
        string cmdExec)
    {
        var description = $"Report Order Schedule ({dbName}:{jobName})";
        await using (var addJobCmd = new SqlCommand(
            "EXEC msdb.dbo.sp_add_job @job_name = @jobName, @enabled = 1, @description = @desc;",
            conn, tx))
        {
            addJobCmd.Parameters.AddWithValue("@jobName", jobName);
            addJobCmd.Parameters.AddWithValue("@desc", description);
            await addJobCmd.ExecuteNonQueryAsync();
        }

        await using (var addStepCmd = new SqlCommand(
            @"EXEC msdb.dbo.sp_add_jobstep
                @job_name = @jobName,
                @step_name = N'RunReportOrder',
                @subsystem = N'CmdExec',
                @command = @cmdExec,
                @on_success_action = 1,
                @on_fail_action = 2,
                @retry_attempts = 0,
                @retry_interval = 0;",
            conn, tx))
        {
            addStepCmd.Parameters.AddWithValue("@jobName", jobName);
            addStepCmd.Parameters.AddWithValue("@cmdExec", cmdExec);
            await addStepCmd.ExecuteNonQueryAsync();
        }

        var scheduleName = jobName.Length > 120 ? jobName[..120] : jobName;
        scheduleName = $"{scheduleName}_Schedule";
        if (scheduleName.Length > 128) scheduleName = scheduleName[..128];

        await using (var addScheduleCmd = new SqlCommand(
            @"EXEC msdb.dbo.sp_add_jobschedule
                @job_name = @jobName,
                @name = @scheduleName,
                @enabled = 1,
                @freq_type = @freqType,
                @freq_interval = @freqInterval,
                @freq_recurrence_factor = @freqRecurrence,
                @active_start_date = @startDate,
                @active_start_time = @startTime;",
            conn, tx))
        {
            addScheduleCmd.Parameters.AddWithValue("@jobName", jobName);
            addScheduleCmd.Parameters.AddWithValue("@scheduleName", scheduleName);
            addScheduleCmd.Parameters.AddWithValue("@freqType", schedule.FreqType);
            addScheduleCmd.Parameters.AddWithValue("@freqInterval", schedule.FreqInterval);
            addScheduleCmd.Parameters.AddWithValue("@freqRecurrence", schedule.FreqRecurrenceFactor);
            addScheduleCmd.Parameters.AddWithValue("@startDate", schedule.StartDate);
            addScheduleCmd.Parameters.AddWithValue("@startTime", schedule.StartTime);
            await addScheduleCmd.ExecuteNonQueryAsync();
        }

        await using var addServerCmd = new SqlCommand("EXEC msdb.dbo.sp_add_jobserver @job_name = @jobName;", conn, tx);
        addServerCmd.Parameters.AddWithValue("@jobName", jobName);
        await addServerCmd.ExecuteNonQueryAsync();
    }

    private string BuildReportOrderScheduleCommand(string itemId)
    {
        var callbackUrl = BuildReportOrderScheduleCallbackUrl();
        var apiKey = (_cfg["ReportOrder:SchedulerApiKey"] ?? string.Empty).Trim();
        var ps = BuildReportOrderPowerShellScript(callbackUrl, itemId, apiKey);
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(ps));
        return $"powershell -NoProfile -ExecutionPolicy Bypass -EncodedCommand {encoded}";
    }

    private string BuildReportOrderScheduleCallbackUrl()
    {
        var explicitUrl = (_cfg["ReportOrder:SchedulerCallbackUrl"] ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(explicitUrl))
            return explicitUrl;

        var host = (_cfg["PcbErpApi:HostAddress"] ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(host))
        {
            var req = HttpContext?.Request;
            if (req != null && req.Host.HasValue)
                host = $"{req.Scheme}://{req.Host.Value}";
        }

        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("找不到 API 位址，請設定 PcbErpApi:HostAddress。");

        return $"{host.TrimEnd('/')}/api/StoredProc/reportOrder/runNowScheduled";
    }

    private static string BuildReportOrderPowerShellScript(string callbackUrl, string itemId, string apiKey)
    {
        var safeUrl = (callbackUrl ?? string.Empty).Replace("'", "''");
        var safeItem = (itemId ?? string.Empty).Replace("'", "''");
        var safeApiKey = (apiKey ?? string.Empty).Replace("'", "''");

        return "$ErrorActionPreference='Stop';"
             + "$ProgressPreference='SilentlyContinue';"
             + $"$body = ConvertTo-Json @{{ itemId = '{safeItem}'; apiKey = '{safeApiKey}' }} -Compress;"
             + $"Invoke-RestMethod -Method Post -Uri '{safeUrl}' -ContentType 'application/json; charset=utf-8' -Body $body | Out-Null;";
    }

    private static async Task<bool> ReportOrderItemExistsAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = "SELECT TOP 1 1 FROM CURdReportOrderBas WITH (NOLOCK) WHERE ItemId = @itemId;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }

    private static async Task<bool> ReportOrderItemFinishedAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = "SELECT TOP 1 1 FROM CURdReportOrderBas WITH (NOLOCK) WHERE ItemId = @itemId AND Finished = 1;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }

    private static async Task<List<ReportOrderCheckRow>> LoadReportOrderCheckRowsAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        var list = new List<ReportOrderCheckRow>();
        await using var cmd = new SqlCommand("CURdReportOrderCheckTime", conn, tx)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 180
        };
        cmd.Parameters.AddWithValue("@ItemId", itemId ?? string.Empty);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var currentItemId = ReadString(rd, "ItemId");
            if (string.IsNullOrWhiteSpace(currentItemId))
                currentItemId = itemId;

            list.Add(new ReportOrderCheckRow(
                ItemId: currentItemId ?? string.Empty,
                OrderExpType: ReadInt(rd, "OrderExpType"),
                PrintRptName: ReadString(rd, "sPrintRptName"),
                ReportPath: ReadString(rd, "ReportOrderPDFPath")
            ));
        }
        return list;
    }

    private static async Task<ReportOrderSqlRow> LoadReportOrderSqlAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        await using var cmd = new SqlCommand("CURdReportOrderGetSQL", conn, tx)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 180
        };
        cmd.Parameters.AddWithValue("@ItemId", itemId ?? string.Empty);

        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync())
            return new ReportOrderSqlRow(string.Empty, string.Empty, string.Empty);

        return new ReportOrderSqlRow(
            SqlText: ReadString(rd, "sSQL"),
            ObjectName: ReadString(rd, "ObjectName"),
            ItemName: ReadString(rd, "ItemName")
        );
    }

    private async Task<string?> BuildReportOrderAttachmentAsync(
        SqlConnection conn,
        SqlTransaction tx,
        ReportOrderCheckRow row,
        ReportOrderSqlRow sqlInfo)
    {
        var reportDir = ResolveWritableReportDir(row.ReportPath);

        // 1: PDF（優先走 Crystal Report API），失敗則回退成 Excel
        if (row.OrderExpType == 1)
        {
            try
            {
                var pdfPath = BuildUniqueAttachmentPath(reportDir, row.ItemId, ".pdf");
                await RenderPdfByCrystalApiAsync(row.PrintRptName, row.ItemId, pdfPath);
                return pdfPath;
            }
            catch
            {
                // fallback
            }
        }

        var dt = await QueryToDataTableAsync(conn, tx, sqlInfo.SqlText);
        if (string.Equals(row.ItemId, "ACA00062", StringComparison.OrdinalIgnoreCase) && dt.Rows.Count == 0)
            return null;

        var xlsPath = BuildUniqueAttachmentPath(reportDir, row.ItemId, ".xls");
        var exportColumns = await LoadExportColumnsAsync(conn, tx, row.ItemId);
        await WriteStyledXlsAsync(dt, xlsPath, exportColumns);
        return xlsPath;
    }

    private async Task RenderPdfByCrystalApiAsync(string printRptName, string itemId, string outPath)
    {
        var reportName = Path.GetFileNameWithoutExtension((printRptName ?? string.Empty).Trim());
        if (string.IsNullOrWhiteSpace(reportName))
            throw new InvalidOperationException("找不到報表名稱(sPrintRptName)");

        var client = _httpFactory.CreateClient("CrystalReport");
        var payload = new
        {
            reportName,
            format = "pdf",
            @params = new Dictionary<string, object>
            {
                ["ItemId"] = itemId ?? string.Empty
            }
        };

        using var resp = await client.PostAsJsonAsync("/api/report/render", payload);
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(err)
                ? $"Crystal API error: {(int)resp.StatusCode}"
                : err);
        }

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        await System.IO.File.WriteAllBytesAsync(outPath, bytes);
    }

    private static async Task<DataTable> QueryToDataTableAsync(SqlConnection conn, SqlTransaction tx, string sqlText)
    {
        if (string.IsNullOrWhiteSpace(sqlText)) return new DataTable();
        await using var cmd = new SqlCommand(sqlText, conn, tx)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 900
        };

        var dt = new DataTable();
        await using var rd = await cmd.ExecuteReaderAsync();
        dt.Load(rd);
        return dt;
    }

    private sealed record ExportColumnDef(string FieldName, string DisplayLabel, string? DataType, string? FormatStr);

    private static async Task<List<ExportColumnDef>> LoadExportColumnsAsync(
        SqlConnection conn,
        SqlTransaction tx,
        string dictTableName)
    {
        var list = new List<ExportColumnDef>();
        if (string.IsNullOrWhiteSpace(dictTableName)) return list;

        const string sql = @"
SELECT f.FieldName,
       COALESCE(NULLIF(l.DisplayLabel,''), NULLIF(f.DisplayLabel,''), f.FieldName) AS DisplayLabel,
       ISNULL(f.DataType, '') AS DataType,
       ISNULL(f.FormatStr, '') AS FormatStr
  FROM CURdTableField f WITH (NOLOCK)
  LEFT JOIN CURdTableFieldLang l WITH (NOLOCK)
    ON l.TableName = f.TableName
   AND l.FieldName = f.FieldName
   AND l.LanguageId = 'TW'
 WHERE f.TableName = @tbl
   AND ISNULL(f.Visible,1) = 1
  ORDER BY ISNULL(f.SerialNum, 99999), f.FieldName;";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@tbl", dictTableName.Trim());
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var field = rd["FieldName"]?.ToString()?.Trim();
            var label = rd["DisplayLabel"]?.ToString()?.Trim();
            var dataType = rd["DataType"]?.ToString()?.Trim();
            var formatStr = rd["FormatStr"]?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(field)) continue;
            if (string.IsNullOrWhiteSpace(label)) label = field;
            if (list.Any(x => string.Equals(x.FieldName, field, StringComparison.OrdinalIgnoreCase))) continue;
            list.Add(new ExportColumnDef(field!, label!, dataType, formatStr));
        }
        return list;
    }

    private static Task WriteStyledXlsAsync(DataTable dt, string filePath, IReadOnlyList<ExportColumnDef> exportColumns)
    {
        var workbook = new HSSFWorkbook();
        var sheet = workbook.CreateSheet("Report");

        var headerStyle = workbook.CreateCellStyle();
        headerStyle.BorderTop = BorderStyle.Thin;
        headerStyle.BorderRight = BorderStyle.Thin;
        headerStyle.BorderBottom = BorderStyle.Thin;
        headerStyle.BorderLeft = BorderStyle.Thin;
        headerStyle.Alignment = HorizontalAlignment.Center;
        headerStyle.VerticalAlignment = VerticalAlignment.Center;
        headerStyle.FillPattern = FillPattern.SolidForeground;
        headerStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;

        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        var bodyStyle = workbook.CreateCellStyle();
        bodyStyle.BorderTop = BorderStyle.Thin;
        bodyStyle.BorderRight = BorderStyle.Thin;
        bodyStyle.BorderBottom = BorderStyle.Thin;
        bodyStyle.BorderLeft = BorderStyle.Thin;
        bodyStyle.VerticalAlignment = VerticalAlignment.Center;

        var allColumns = dt.Columns.Cast<DataColumn>().ToList();
        var selectedColumns = new List<(DataColumn Column, ExportColumnDef Def)>();
        if (exportColumns != null && exportColumns.Count > 0)
        {
            foreach (var cfg in exportColumns)
            {
                var hit = allColumns.FirstOrDefault(c => string.Equals(c.ColumnName, cfg.FieldName, StringComparison.OrdinalIgnoreCase));
                if (hit == null) continue;
                selectedColumns.Add((hit, cfg));
            }
        }
        if (selectedColumns.Count == 0)
        {
            selectedColumns.AddRange(allColumns.Select(c =>
                (c, new ExportColumnDef(c.ColumnName, c.ColumnName, c.DataType?.Name, string.Empty))));
        }

        var widthLen = new int[selectedColumns.Count];
        var headerRow = sheet.CreateRow(0);
        for (var c = 0; c < selectedColumns.Count; c++)
        {
            var title = selectedColumns[c].Def.DisplayLabel;
            var cell = headerRow.CreateCell(c);
            cell.SetCellValue(title);
            cell.CellStyle = headerStyle;
            widthLen[c] = CalcDisplayLength(title);
        }

        for (var r = 0; r < dt.Rows.Count; r++)
        {
            var row = sheet.CreateRow(r + 1);
            var dr = dt.Rows[r];
            for (var c = 0; c < selectedColumns.Count; c++)
            {
                var val = dr[selectedColumns[c].Column];
                var cell = row.CreateCell(c);
                var text = FormatExportText(val, selectedColumns[c].Def);
                cell.SetCellValue(SanitizeCell(text));
                cell.CellStyle = bodyStyle;
                var visualLen = CalcDisplayLength(text);
                if (visualLen > widthLen[c]) widthLen[c] = visualLen;
            }
        }

        // 快速欄寬計算（比 AutoSizeColumn 快很多）
        for (var c = 0; c < selectedColumns.Count; c++)
        {
            var width = (Math.Min(widthLen[c] + 2, 60)) * 256;
            var minWidth = 12 * 256;
            var maxWidth = 60 * 256;
            if (width < minWidth) sheet.SetColumnWidth(c, minWidth);
            else if (width > maxWidth) sheet.SetColumnWidth(c, maxWidth);
            else sheet.SetColumnWidth(c, width);
        }
        sheet.CreateFreezePane(0, 1);

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        workbook.Write(fs);
        return Task.CompletedTask;
    }

    private static int CalcDisplayLength(string? text)
    {
        var s = text ?? string.Empty;
        var len = 0;
        foreach (var ch in s)
            len += ch > 255 ? 2 : 1;
        return len;
    }

    private static string FormatExportText(object? raw, ExportColumnDef col)
    {
        if (raw == null || raw == DBNull.Value) return string.Empty;

        var dataType = (col.DataType ?? string.Empty).Trim().ToLowerInvariant();
        var s = Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;

        if (IsDateType(dataType) || raw is DateTime || DateTime.TryParse(s, out _))
        {
            if (!TryGetDateTime(raw, out var dt) && !DateTime.TryParse(s, out dt))
                return s;

            // 避免出現「上午 12:00:00」：整點 00:00:00 一律只顯示日期
            if (dt.TimeOfDay == TimeSpan.Zero)
                return dt.ToString("yyyy/M/d", CultureInfo.InvariantCulture);

            return dt.ToString("yyyy/M/d HH:mm:ss", CultureInfo.InvariantCulture);
        }

        if (IsNumericType(dataType) || IsNumericValue(raw) || decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
        {
            if (!TryGetDecimal(raw, out var num) && !decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
                return s;

            // 去除尾端無效 0，避免 100.00000000
            if (num == decimal.Truncate(num))
                return num.ToString("0", CultureInfo.InvariantCulture);
            return num.ToString("0.########", CultureInfo.InvariantCulture);
        }

        return s;
    }

    private static bool IsDateType(string dt)
    {
        if (string.IsNullOrWhiteSpace(dt)) return false;
        return dt.Contains("date", StringComparison.OrdinalIgnoreCase)
            || dt.Contains("time", StringComparison.OrdinalIgnoreCase)
            || dt.Equals("datetime2", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNumericType(string dt)
    {
        if (string.IsNullOrWhiteSpace(dt)) return false;
        return dt.Contains("int", StringComparison.OrdinalIgnoreCase)
            || dt.Contains("dec", StringComparison.OrdinalIgnoreCase)
            || dt.Contains("num", StringComparison.OrdinalIgnoreCase)
            || dt.Contains("float", StringComparison.OrdinalIgnoreCase)
            || dt.Contains("double", StringComparison.OrdinalIgnoreCase)
            || dt.Contains("money", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNumericValue(object raw)
    {
        return raw is byte or sbyte
            or short or ushort
            or int or uint
            or long or ulong
            or float or double
            or decimal;
    }

    private static bool TryGetDateTime(object raw, out DateTime dt)
    {
        if (raw is DateTime d)
        {
            dt = d;
            return true;
        }
        if (raw is DateTimeOffset dto)
        {
            dt = dto.DateTime;
            return true;
        }
        return DateTime.TryParse(Convert.ToString(raw), out dt);
    }

    private static bool TryGetDecimal(object raw, out decimal d)
    {
        switch (raw)
        {
            case decimal dec:
                d = dec;
                return true;
            case byte b:
                d = b;
                return true;
            case sbyte sb:
                d = sb;
                return true;
            case short s:
                d = s;
                return true;
            case ushort us:
                d = us;
                return true;
            case int i:
                d = i;
                return true;
            case uint ui:
                d = ui;
                return true;
            case long l:
                d = l;
                return true;
            case ulong ul:
                d = ul;
                return true;
            case float f:
                d = Convert.ToDecimal(f);
                return true;
            case double db:
                d = Convert.ToDecimal(db);
                return true;
            default:
                return decimal.TryParse(Convert.ToString(raw), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
        }
    }

    private static string SanitizeCell(string s)
    {
        return (s ?? string.Empty)
            .Replace("\r\n", " ")
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ');
    }

    private static async Task SendReportOrderMailAsync(SqlConnection conn, SqlTransaction tx, string itemId, string attFileName)
    {
        await using var cmd = new SqlCommand("CURdReportOrderSendMail", conn, tx)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 180
        };
        cmd.Parameters.AddWithValue("@ItemId", itemId ?? string.Empty);
        cmd.Parameters.AddWithValue("@AttFileName", attFileName ?? string.Empty);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string NormalizeDir(string? dir)
    {
        var s = (dir ?? string.Empty).Trim().Trim('"');
        return string.IsNullOrWhiteSpace(s) ? string.Empty : s;
    }

    private static string BuildUniqueAttachmentPath(string reportDir, string? itemId, string ext)
    {
        var rawName = string.IsNullOrWhiteSpace(itemId) ? "ReportOrder" : itemId.Trim();
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(rawName.Length);
        foreach (var ch in rawName)
        {
            sb.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
        }

        var safeName = sb.ToString().Trim();
        if (string.IsNullOrWhiteSpace(safeName))
            safeName = "ReportOrder";

        var extension = string.IsNullOrWhiteSpace(ext)
            ? ".dat"
            : (ext.StartsWith('.') ? ext : "." + ext);

        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        var token = Guid.NewGuid().ToString("N").Substring(0, 8);
        return Path.Combine(reportDir, $"{safeName}_{stamp}_{token}{extension}");
    }

    private string ResolveWritableReportDir(string? preferredPath)
    {
        var candidates = new List<string>();

        var preferred = NormalizeDir(preferredPath);
        if (!string.IsNullOrWhiteSpace(preferred))
            candidates.Add(preferred);

        var cfgPath = NormalizeDir(_cfg["ReportOrder:OutputPath"]);
        if (!string.IsNullOrWhiteSpace(cfgPath)
            && !candidates.Any(x => string.Equals(x, cfgPath, StringComparison.OrdinalIgnoreCase)))
        {
            candidates.Add(cfgPath);
        }

        foreach (var path in candidates)
        {
            try
            {
                Directory.CreateDirectory(path);
                return path;
            }
            catch
            {
                // try next candidate
            }
        }

        throw new InvalidOperationException(
            $"報表輸出路徑不可用。原設定：{preferredPath ?? "(空白)"}。請設定可由 Web 與 SQL Server 同時存取的共享路徑(UNC)到 appsettings 的 ReportOrder:OutputPath，例如 \\\\fileserver\\ReportOrder\\Out");
    }

    private static int FindOrdinal(IDataRecord r, string columnName)
    {
        for (var i = 0; i < r.FieldCount; i++)
        {
            if (string.Equals(r.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private static string ReadString(IDataRecord r, string columnName)
    {
        var ord = FindOrdinal(r, columnName);
        if (ord < 0 || r.IsDBNull(ord)) return string.Empty;
        return Convert.ToString(r.GetValue(ord)) ?? string.Empty;
    }

    private static int ReadInt(IDataRecord r, string columnName)
    {
        var ord = FindOrdinal(r, columnName);
        if (ord < 0 || r.IsDBNull(ord)) return 0;
        return int.TryParse(Convert.ToString(r.GetValue(ord)), out var n) ? n : 0;
    }

    private static int? ReadNullableInt(IDataRecord r, string columnName)
    {
        var ord = FindOrdinal(r, columnName);
        if (ord < 0 || r.IsDBNull(ord)) return null;
        if (int.TryParse(Convert.ToString(r.GetValue(ord)), out var n)) return n;
        return null;
    }

    private sealed record ButtonRow(string ItemId, string ButtonName, int DesignType, string? SpName, string? ExecSpName);
    private sealed record ButtonParamRow(int SeqNum, int? ParamType, string? TableKind, string? ParamFieldName, string? ClientTblName);
    private sealed record TableKindMap(string Kind, string TableName);

    private static string NormalizeIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        return name.Trim();
    }

    private static string QuoteIdentifier(string name)
    {
        var n = NormalizeIdentifier(name);
        if (string.IsNullOrWhiteSpace(n)) return string.Empty;
        var parts = n.Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(p, "^[A-Za-z_][A-Za-z0-9_]*$"))
                throw new InvalidOperationException("非法的資料表或欄位名稱");
        }
        return string.Join(".", parts.Select(p => $"[{p}]"));
    }

    private static string NormalizeTableKind(string? kind)
    {
        return (kind ?? string.Empty).Trim();
    }

    private async Task<ButtonRow?> LoadButtonAsync(SqlConnection conn, SqlTransaction tx, string itemId, string buttonName)
    {
        const string sql = @"
SELECT TOP 1 ItemId, ButtonName, ISNULL(DesignType,0) AS DesignType, SpName, ExecSpName
  FROM CURdOCXItemCustButton WITH (NOLOCK)
 WHERE ItemId = @itemId AND ButtonName = @buttonName;";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        cmd.Parameters.AddWithValue("@buttonName", buttonName);
        await using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return null;
        return new ButtonRow(
            rd["ItemId"]?.ToString() ?? string.Empty,
            rd["ButtonName"]?.ToString() ?? string.Empty,
            Convert.ToInt32(rd["DesignType"] ?? 0),
            rd["SpName"]?.ToString(),
            rd["ExecSpName"]?.ToString()
        );
    }

    private async Task<List<ButtonParamRow>> LoadButtonParamsAsync(SqlConnection conn, SqlTransaction tx, string itemId, string buttonName, int opKind)
    {
        var list = new List<ButtonParamRow>();
        if (await TryLoadButtonParamsByProc(conn, tx, itemId, buttonName, opKind, list))
        {
            return list;
        }
        const string sql = @"
SELECT SeqNum, ParamType, TableKind, ParamFieldName
  FROM CURdOCXItmCusBtnParam WITH (NOLOCK)
 WHERE ItemId = @itemId AND ButtonName = @buttonName
 ORDER BY SeqNum, Seq;";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        cmd.Parameters.AddWithValue("@buttonName", buttonName);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new ButtonParamRow(
                SeqNum: Convert.ToInt32(rd["SeqNum"] ?? 0),
                ParamType: rd["ParamType"] == DBNull.Value ? null : Convert.ToInt32(rd["ParamType"]),
                TableKind: rd["TableKind"]?.ToString(),
                ParamFieldName: rd["ParamFieldName"]?.ToString(),
                ClientTblName: null
            ));
        }
        return list;
    }

    private static int GetOpKind(Dictionary<string, object>? args)
    {
        if (args == null) return 1;
        var entry = args.FirstOrDefault(k => string.Equals(k.Key, "opKind", StringComparison.OrdinalIgnoreCase));
        if (entry.Key == null) return 1;
        var v = entry.Value;
        if (v is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out var n)) return n;
            if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out var ns)) return ns;
        }
        return int.TryParse(v?.ToString(), out var n2) ? n2 : 1;
    }

    private static async Task<bool> TryLoadButtonParamsByProc(
        SqlConnection conn,
        SqlTransaction tx,
        string itemId,
        string buttonName,
        int opKind,
        List<ButtonParamRow> list)
    {
        const string sql = "exec CURdOCXItmCusBtnParamGet @p1, @p2, @p3";
        try
        {
            await using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@p1", itemId);
            cmd.Parameters.AddWithValue("@p2", buttonName);
            cmd.Parameters.AddWithValue("@p3", opKind);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new ButtonParamRow(
                    SeqNum: Convert.ToInt32(rd["SeqNum"] ?? 0),
                    ParamType: rd["ParamType"] == DBNull.Value ? null : Convert.ToInt32(rd["ParamType"]),
                    TableKind: rd["TableKind"]?.ToString(),
                    ParamFieldName: rd["ParamFieldName"]?.ToString(),
                    ClientTblName: rd["ClientTblName"]?.ToString()
                ));
            }
            return list.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<Dictionary<string, string>> LoadTableMapAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = @"
SELECT TableKind, TableName
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE ItemId = @itemId;";

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var kind = NormalizeTableKind(rd["TableKind"]?.ToString());
            var table = rd["TableName"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(table)) continue;
            if (!map.ContainsKey(kind)) map[kind] = table;
        }
        return map;
    }

    private async Task EnsureSendOrderSourceFlagsAsync(
        SqlConnection conn,
        SqlTransaction tx,
        Dictionary<string, string> tableMap,
        string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum)) return;

        var masterTable = ResolveTableName(tableMap, "Master1");
        if (string.IsNullOrWhiteSpace(masterTable))
            masterTable = ResolveTableName(tableMap, "Master");
        if (string.IsNullOrWhiteSpace(masterTable)) return;

        var actualTable = await ResolveRealTableNameAsync(conn, tx, masterTable) ?? masterTable;
        var safeTable = QuoteIdentifier(actualTable);
        if (string.IsNullOrWhiteSpace(safeTable)) return;

        var cols = await LoadTableColumnsAsync(conn, tx, actualTable);
        if (!cols.Contains("PaperNum") ||
            !cols.Contains("UseRecent") ||
            !cols.Contains("UseQuota") ||
            !cols.Contains("UseTable") ||
            !cols.Contains("IsPost"))
        {
            return;
        }

        var where = @"
[PaperNum] = @paperNum
AND ISNULL([UseRecent], 0) = 0
AND ISNULL([UseTable], 0) = 0";
        if (cols.Contains("Finished"))
            where += "\nAND ISNULL([Finished], 0) = 0";

        var sql = $@"
UPDATE {safeTable}
   SET [UseRecent] = CASE WHEN ISNULL([UseRecent], 0) = 0 THEN 1 ELSE [UseRecent] END,
       [UseQuota] = CASE WHEN ISNULL([UseQuota], 0) = 0 THEN 1 ELSE [UseQuota] END,
       [UseTable] = CASE WHEN ISNULL([UseTable], 0) = 0 THEN 1 ELSE [UseTable] END,
       [IsPost] = CASE WHEN ISNULL([IsPost], 0) = 0 THEN 1 ELSE [IsPost] END
 WHERE {where};";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@paperNum", paperNum);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<HashSet<string>> LoadTableColumnsAsync(SqlConnection conn, SqlTransaction tx, string tableName)
    {
        static string Normalize(string s)
        {
            return (s ?? string.Empty).Trim().Replace("[", string.Empty).Replace("]", string.Empty);
        }

        static async Task<HashSet<string>> QueryAsync(SqlConnection c, SqlTransaction t, string objName)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            const string sql = "SELECT name FROM sys.columns WITH (NOLOCK) WHERE object_id = OBJECT_ID(@objName);";
            await using var cmd = new SqlCommand(sql, c, t);
            cmd.Parameters.AddWithValue("@objName", objName ?? string.Empty);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var col = rd["name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(col)) set.Add(col);
            }
            return set;
        }

        var normalized = Normalize(tableName);
        if (string.IsNullOrWhiteSpace(normalized))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var set1 = await QueryAsync(conn, tx, normalized);
        if (set1.Count > 0) return set1;

        if (!normalized.Contains('.'))
        {
            var set2 = await QueryAsync(conn, tx, "dbo." + normalized);
            if (set2.Count > 0) return set2;
        }

        return set1;
    }

    private async Task<string?> LoadSystemIdAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = "SELECT TOP 1 SystemId FROM CURdSysItems WITH (NOLOCK) WHERE ItemId = @itemId;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private async Task<object?> ResolveParamValueAsync(
        SqlConnection conn,
        SqlTransaction tx,
        Dictionary<string, string> tableMap,
        string paperNum,
        ButtonParamRow p,
        string? systemId,
        string userId,
        Dictionary<string, object>? args)
    {
        var paramType = p.ParamType ?? 0;
        switch (paramType)
        {
            case 0: // 欄位值
            {
                if (string.IsNullOrWhiteSpace(p.ParamFieldName)) return null;
                var rowValue = ReadFieldFromArgs(args, p.ClientTblName, p.TableKind, p.ParamFieldName);
                if (rowValue != null) return rowValue;
                var tableKind = NormalizeTableKind(p.TableKind);
                if (string.IsNullOrWhiteSpace(tableKind)) tableKind = "Master1";
                var tableName = ResolveTableName(tableMap, tableKind);
                if (string.IsNullOrWhiteSpace(tableName)) return null;
                return await ReadFieldValueAsync(conn, tx, tableName, p.ParamFieldName!, paperNum);
            }
            case 1: // 常數
                return p.ParamFieldName ?? string.Empty;
            case 2: // 登入者工號
                return string.IsNullOrWhiteSpace(userId) ? null : userId;
            case 3: // 公司別
                return "A001";
            case 4: // 系統別
                return systemId ?? string.Empty;
            case 5: // 目前單號
                return paperNum;
            default:
                return null;
        }
    }

    private static object? ReadFieldFromArgs(
        Dictionary<string, object>? args,
        string? clientTblName,
        string? tableKind,
        string fieldName)
    {
        if (args == null) return null;
        var masterRow = GetArgValue(args, "masterRow");
        var detailRow = GetArgValue(args, "detailRow");

        object? row = null;
        var name = (clientTblName ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            row = GetArgValue(args, name);
            if (row == null)
            {
                if (name.Contains("master", StringComparison.OrdinalIgnoreCase) || name.Contains("mas", StringComparison.OrdinalIgnoreCase))
                    row = masterRow;
                else if (name.Contains("detail", StringComparison.OrdinalIgnoreCase) || name.Contains("dtl", StringComparison.OrdinalIgnoreCase))
                    row = detailRow;
            }
        }
        if (row == null && !string.IsNullOrWhiteSpace(tableKind))
        {
            var k = tableKind.Trim();
            if (k.StartsWith("Master", StringComparison.OrdinalIgnoreCase)) row = masterRow;
            else if (k.StartsWith("Detail", StringComparison.OrdinalIgnoreCase)) row = detailRow;
        }
        if (row == null) return null;
        return ReadFieldFromRow(row, fieldName);
    }

    private static object? GetArgValue(Dictionary<string, object> args, string key)
    {
        var hit = args.FirstOrDefault(k => string.Equals(k.Key, key, StringComparison.OrdinalIgnoreCase));
        return hit.Key == null ? null : hit.Value;
    }

    private static object? ReadFieldFromRow(object row, string fieldName)
    {
        if (row is JsonElement je)
        {
            if (je.ValueKind != JsonValueKind.Object) return null;
            foreach (var prop in je.EnumerateObject())
            {
                if (string.Equals(prop.Name, fieldName, StringComparison.OrdinalIgnoreCase))
                    return ToClr(prop.Value);
            }
            return null;
        }
        if (row is Dictionary<string, object> dict)
        {
            foreach (var kv in dict)
                if (string.Equals(kv.Key, fieldName, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
        }
        return null;
    }

    private static string? ResolveTableName(Dictionary<string, string> tableMap, string tableKind)
    {
        if (tableMap.TryGetValue(tableKind, out var name)) return name;

        var k = tableKind.Trim();
        if (k.StartsWith("Master", StringComparison.OrdinalIgnoreCase))
        {
            var master = tableMap
                .FirstOrDefault(x => x.Key.Contains("Master", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(master.Value) ? null : master.Value;
        }

        if (k.StartsWith("Detail", StringComparison.OrdinalIgnoreCase))
        {
            var digit = new string(k.SkipWhile(c => !char.IsDigit(c)).ToArray());
            if (!string.IsNullOrWhiteSpace(digit))
            {
                var match = tableMap.FirstOrDefault(x => x.Key.Equals($"Detail{digit}", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(match.Value)) return match.Value;
            }
            var firstDetail = tableMap.FirstOrDefault(x => x.Key.StartsWith("Detail", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(firstDetail.Value) ? null : firstDetail.Value;
        }

        return null;
    }

    private async Task<object?> ReadFieldValueAsync(SqlConnection conn, SqlTransaction tx, string tableName, string fieldName, string paperNum)
    {
        var actualTable = await ResolveRealTableNameAsync(conn, tx, tableName) ?? tableName;
        var safeTable = QuoteIdentifier(actualTable);
        var safeField = QuoteIdentifier(fieldName);
        if (string.IsNullOrWhiteSpace(safeTable) || string.IsNullOrWhiteSpace(safeField)) return null;

        if (string.IsNullOrWhiteSpace(paperNum)) return null;
        var sql = $"SELECT TOP 1 {safeField} FROM {safeTable} WITH (NOLOCK) WHERE [PaperNum] = @paperNum";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@paperNum", paperNum ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == DBNull.Value ? null : obj;
    }

    private async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, SqlTransaction tx, string dictTableName)
    {
        const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl;";

        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        if (obj == null || obj == DBNull.Value) return null;
        return obj.ToString();
    }

    private async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, string dictTableName)
    {
        const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl;";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        if (obj == null || obj == DBNull.Value) return null;
        return obj.ToString();
    }

    /// <summary>
    /// 直接查詢資料表
    /// POST /api/StoredProc/queryDirect
    /// Body: { TableName, WhereClause?, Parameters?, Columns? }
    /// </summary>
    [HttpPost("queryDirect")]
    public async Task<IActionResult> QueryDirect([FromBody] QueryDirectRequest req)
    {
        Console.WriteLine($"[StoredProc.QueryDirect] Table={req?.TableName}, Where={req?.WhereClause}");

        if (string.IsNullOrWhiteSpace(req?.TableName))
            return BadRequest(new { ok = false, error = "TableName 為必填" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            // 安全處理表名（防止 SQL Injection）
            var safeTable = QuoteIdentifier(req.TableName);
            if (string.IsNullOrWhiteSpace(safeTable))
                return BadRequest(new { ok = false, error = "無效的表名" });

            // 處理要查詢的欄位
            var columns = "*";
            if (req.Columns != null && req.Columns.Length > 0)
            {
                var safeColumns = req.Columns.Select(c => QuoteIdentifier(c)).Where(c => !string.IsNullOrWhiteSpace(c));
                columns = string.Join(", ", safeColumns);
                if (string.IsNullOrWhiteSpace(columns))
                    columns = "*";
            }

            // 構建 SQL
            var sql = $"SELECT {columns} FROM {safeTable} WITH (NOLOCK)";

            // 加上 WHERE 條件
            if (!string.IsNullOrWhiteSpace(req.WhereClause))
            {
                sql += $" WHERE {req.WhereClause}";
            }

            await using var cmd = new SqlCommand(sql, conn, tx)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 120
            };

            // 添加參數（防止 SQL Injection）
            if (req.Parameters != null)
            {
                foreach (var param in req.Parameters)
                {
                    var paramName = param.Key.StartsWith("@") ? param.Key : "@" + param.Key;
                    var paramValue = ToClr(param.Value) ?? DBNull.Value;
                    cmd.Parameters.AddWithValue(paramName, paramValue);
                }
            }

            // 執行查詢
            var results = new List<Dictionary<string, object?>>();

            // 使用 using 塊確保 reader 在提交事務前完全釋放
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[name] = value;
                    }
                    results.Add(row);
                }
            }  // reader 在此處被釋放

            await tx.CommitAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    // Helper: 將 JsonElement 轉成 CLR 類型
    private static object? ToClr(object? v)
    {
        if (v is null) return DBNull.Value;
        if (v is not JsonElement je) return v;

        return je.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
            JsonValueKind.String => je.GetString(),
            JsonValueKind.Number => je.TryGetInt64(out var l) ? l :
                                    je.TryGetDecimal(out var d) ? d :
                                    je.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => je.GetRawText()
        };
    }
}
