using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StoredProcController : ControllerBase
{
    private readonly string _cs;
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
        )

        };

    public StoredProcController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? cfg.GetConnectionString("Default")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到 ConnectionStrings:Default 或 DbContext 連線字串");
    }

    public record ExecSpRequest(string Key, Dictionary<string, object>? Args);
    public record ExecByButtonRequest(string ItemId, string ButtonName, string PaperNum, Dictionary<string, object>? Args);
    public record QueryDirectRequest(string TableName, string? WhereClause, Dictionary<string, object>? Parameters, string[]? Columns);
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

            // 使用 ExecuteReaderAsync 讀取結果集
            var results = new List<Dictionary<string, object?>>();

            // 使用 using 塊確保 reader 在提交事務前完全釋放
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                // 讀取第一個結果集的所有行
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

                // 確保讀取完所有結果集，避免 DataReader 保持打開狀態
                while (await reader.NextResultAsync())
                {
                    // 消耗其他結果集，但不處理它們
                    while (await reader.ReadAsync()) { }
                }
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

            var spName = QuoteIdentifier(spNameRaw);
            var opKind = GetOpKind(req.Args);
            var paramDefs = await LoadButtonParamsAsync(conn, tx, req.ItemId, req.ButtonName, opKind);

            var tableMap = await LoadTableMapAsync(conn, tx, req.ItemId);

            var systemId = await LoadSystemIdAsync(conn, tx, req.ItemId);
            var userId = User?.Identity?.Name ?? string.Empty;

            var placeholders = paramDefs.Count == 0
                ? string.Empty
                : " " + string.Join(", ", Enumerable.Range(1, paramDefs.Count).Select(i => $"@p{i}"));

            await using var cmd = new SqlCommand($"EXEC {spName}{placeholders}", conn, tx)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 120
            };

            for (var i = 0; i < paramDefs.Count; i++)
            {
                var p = paramDefs[i];
                var value = await ResolveParamValueAsync(conn, tx, tableMap, req.PaperNum, p, systemId, userId, req.Args);
                cmd.Parameters.AddWithValue($"@p{i + 1}", value ?? DBNull.Value);
            }

            var affected = await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
            return Ok(new { ok = true, rowsAffected = affected });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
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
