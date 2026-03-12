using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Linq;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class MG000006Controller : ControllerBase
{
    private readonly string _connStr;
    private readonly PcbErpContext _db;
    private readonly ILogger<MG000006Controller> _logger;

    public MG000006Controller(IConfiguration cfg, PcbErpContext db, ILogger<MG000006Controller> logger)
    {
        _db = db;
        _connStr = cfg.GetConnectionString("Default")
                   ?? db?.Database.GetDbConnection().ConnectionString
                   ?? throw new InvalidOperationException("蝻箏?鞈?摨恍??摮葡");
        _logger = logger;
    }

    public record KeyRequest(string? SetClass, string? NumId);
    public record TestRequest(string? SetClass);
    public record ImportRequest(string? SetClass, string? NumId);
    public record CopyRequest(string? SetClass, string? NumId, string? UserId, int? IsMust, int? IsHand);
    public record DictWidthDto(string TableName, string FieldName, int Width);

    [HttpGet("mat-class")]
    public async Task<ActionResult<IEnumerable<IDictionary<string, object?>>>> GetMatClassAsync()
    {
        const string sql = @"SELECT * FROM dbo.MINdMatClass WITH (NOLOCK) ORDER BY MatClass";
        var list = await QueryListAsync(sql);

        // 與 Client / 動態模板一致：先補 OCX Lookup 非實體欄位
        try
        {
            var dictService = new TableDictionaryService(_db);
            var dictCandidates = new[] { "MGN_MINdMatClass", "MGN_MINMatClass", "MINdMatClass" };
            List<TableDictionaryService.OCXLookupMap> lookupMaps = new();
            foreach (var dictTable in dictCandidates)
            {
                lookupMaps = dictService.GetOCXLookups(dictTable);
                if (lookupMaps.Count > 0) break;
            }

            if (lookupMaps.Count > 0)
            {
                static string ToKey(object? v) => v == null || v == DBNull.Value ? "" : v.ToString()?.Trim() ?? "";

                foreach (var row in list)
                {
                    foreach (var map in lookupMaps)
                    {
                        if (map == null || string.IsNullOrWhiteSpace(map.FieldName)) continue;
                        if (row.ContainsKey(map.FieldName)) continue;

                        var key = "";
                        if (!string.IsNullOrWhiteSpace(map.KeyFieldName) && row.TryGetValue(map.KeyFieldName, out var keyFieldVal))
                            key = ToKey(keyFieldVal);
                        if (string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(map.KeySelfName) && row.TryGetValue(map.KeySelfName, out var keySelfVal))
                            key = ToKey(keySelfVal);
                        if (string.IsNullOrWhiteSpace(key) && row.TryGetValue(map.FieldName, out var rawVal))
                            key = ToKey(rawVal);

                        var display = "";
                        if (!string.IsNullOrWhiteSpace(key) && map.LookupValues != null && map.LookupValues.TryGetValue(key, out var dv) && dv != null)
                            display = dv;

                        row[map.FieldName] = display;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MG000006 mat-class: build OCX lookup display failed");
        }

        // 通用補值：依字典 Lookup，自動回填所有 Lk_xxxName（一次套用）
        await FillLookupNameFallbackAsync(list, "MGN_MINdMatClass", "MGN_MINMatClass", "MINdMatClass");

        return Ok(list);
    }

    [HttpDelete("mat-class/{matClass}")]
    public async Task<IActionResult> DeleteMatClassAsync(string matClass)
    {
        if (string.IsNullOrWhiteSpace(matClass))
            return BadRequest(new { ok = false, error = "蝻箏? MatClass" });

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("DELETE FROM dbo.MINdMatClass WHERE MatClass = @matClass", conn);
            cmd.Parameters.Add(new SqlParameter("@matClass", SqlDbType.VarChar, 8) { Value = matClass.Trim() });
            var affected = await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = affected > 0, affected });
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "MG000006 delete mat-class failed, MatClass={MatClass}", matClass);
            return ErrorText(StatusCodes.Status400BadRequest, FirstLine(ex.Message));
        }
    }

    [HttpGet("setnum-main")]
    public async Task<ActionResult<IEnumerable<IDictionary<string, object?>>>> GetSetNumMainAsync()
    {
        const string sql = @"
SELECT m.*,
       c.ClassName
  FROM dbo.MGNdSetNumMain m WITH (NOLOCK)
  LEFT JOIN dbo.MINdMatClass c WITH (NOLOCK)
    ON c.MatClass = m.SetClass
 ORDER BY m.SetClass";
        var list = await QueryListAsync(sql);
        return Ok(list);
    }

    [HttpDelete("setnum-main/{setClass}")]
    public async Task<IActionResult> DeleteSetNumMainAsync(string setClass)
    {
        if (string.IsNullOrWhiteSpace(setClass))
            return BadRequest(new { ok = false, error = "蝻箏? SetClass" });

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("DELETE FROM dbo.MGNdSetNumMain WHERE SetClass = @setClass", conn);
            cmd.Parameters.Add(new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() });
            var affected = await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = affected > 0, affected });
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "MG000006 delete setnum-main failed, SetClass={SetClass}", setClass);
            return ErrorText(StatusCodes.Status400BadRequest, FirstLine(ex.Message));
        }
    }

    [HttpGet("setnum-sub")]
    public async Task<ActionResult<IEnumerable<IDictionary<string, object?>>>> GetSetNumSubAsync([FromQuery] string? setClass)
    {
        if (string.IsNullOrWhiteSpace(setClass))
            return BadRequest(new { ok = false, error = "蝻箏? SetClass" });

        const string sql = @"SELECT * FROM dbo.MGNdSetNumSub WITH (NOLOCK) WHERE SetClass = @setClass ORDER BY NumId";
        var list = await QueryListAsync(sql, new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() });
        return Ok(list);
    }

    [HttpDelete("setnum-sub")]
    public async Task<IActionResult> DeleteSetNumSubAsync([FromQuery] string? setClass, [FromQuery] string? numId)
    {
        if (string.IsNullOrWhiteSpace(setClass) || string.IsNullOrWhiteSpace(numId))
            return BadRequest(new { ok = false, error = "蝻箏? SetClass ??NumId" });

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("DELETE FROM dbo.MGNdSetNumSub WHERE SetClass = @setClass AND NumId = @numId", conn);
            cmd.Parameters.Add(new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() });
            cmd.Parameters.Add(new SqlParameter("@numId", SqlDbType.Char, 1) { Value = numId.Trim() });
            var affected = await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = affected > 0, affected });
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "MG000006 delete setnum-sub failed, SetClass={SetClass}, NumId={NumId}", setClass, numId);
            return ErrorText(StatusCodes.Status400BadRequest, FirstLine(ex.Message));
        }
    }

    [HttpGet("setnum-subdtl")]
    public async Task<ActionResult<IEnumerable<IDictionary<string, object?>>>> GetSetNumSubDtlAsync([FromQuery] string? setClass, [FromQuery] string? numId)
    {
        if (string.IsNullOrWhiteSpace(setClass) || string.IsNullOrWhiteSpace(numId))
            return BadRequest(new { ok = false, error = "蝻箏? SetClass ??NumId" });

        const string sql = @"SELECT * FROM dbo.MGNdSetNumSubDtl WITH (NOLOCK) WHERE SetClass = @setClass AND NumId = @numId ORDER BY EnCode";
        var list = await QueryListAsync(
            sql,
            new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() },
            new SqlParameter("@numId", SqlDbType.Char, 1) { Value = numId.Trim() });
        return Ok(list);
    }

    [HttpDelete("setnum-subdtl")]
    public async Task<IActionResult> DeleteSetNumSubDtlAsync([FromQuery] string? setClass, [FromQuery] string? numId, [FromQuery] string? encode)
    {
        if (string.IsNullOrWhiteSpace(setClass) || string.IsNullOrWhiteSpace(numId) || string.IsNullOrWhiteSpace(encode))
            return BadRequest(new { ok = false, error = "蝻箏? SetClass / NumId / EnCode" });

        try
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("DELETE FROM dbo.MGNdSetNumSubDtl WHERE SetClass = @setClass AND NumId = @numId AND EnCode = @encode", conn);
            cmd.Parameters.Add(new SqlParameter("@setClass", SqlDbType.VarChar, 12) { Value = setClass.Trim() });
            cmd.Parameters.Add(new SqlParameter("@numId", SqlDbType.Char, 1) { Value = numId.Trim() });
            cmd.Parameters.Add(new SqlParameter("@encode", SqlDbType.VarChar, 24) { Value = encode.Trim() });
            var affected = await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = affected > 0, affected });
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "MG000006 delete setnum-subdtl failed, SetClass={SetClass}, NumId={NumId}, EnCode={EnCode}", setClass, numId, encode);
            return ErrorText(StatusCodes.Status400BadRequest, FirstLine(ex.Message));
        }
    }

    [HttpPost("test-number")]
    public async Task<ActionResult<object>> TestNumberAsync([FromBody] TestRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.SetClass))
            return BadRequest(new { ok = false, error = "蝻箏? SetClass" });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdGenSetTestNum", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add(new SqlParameter("@SetClass", SqlDbType.VarChar, 12) { Value = req.SetClass.Trim() });

        try
        {
            var result = await cmd.ExecuteScalarAsync();
            return Ok(new { ok = true, result = result?.ToString() ?? string.Empty });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MG000006 test-number failed");
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
        }
    }

    [HttpPost("import-mapping")]
    public async Task<IActionResult> ImportMappingAsync([FromBody] ImportRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.SetClass) || string.IsNullOrWhiteSpace(req?.NumId))
            return BadRequest(new { ok = false, error = "蝻箏? SetClass ??NumId" });

        var sql = "exec MGNdSetNumSubDtlImp @SetClass, @NumId";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@SetClass", SqlDbType.VarChar, 12) { Value = req.SetClass.Trim() });
        cmd.Parameters.Add(new SqlParameter("@NumId", SqlDbType.Char, 1) { Value = req.NumId.Trim() });

        try
        {
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MG000006 import-mapping failed, SetClass={SetClass}, NumId={NumId}", req.SetClass, req.NumId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
        }
    }

    [HttpPost("copy-to-mat")]
    public async Task<IActionResult> CopyToMatAsync([FromBody] CopyRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.SetClass) || string.IsNullOrWhiteSpace(req?.NumId))
            return BadRequest(new { ok = false, error = "蝻箏? SetClass ??NumId" });

        var sql = "exec MGNdSetNumSubCopyToMat @SetClass, @NumId, @UserId, @IsMust, @IsHand";

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@SetClass", SqlDbType.VarChar, 12) { Value = req.SetClass.Trim() });
        cmd.Parameters.Add(new SqlParameter("@NumId", SqlDbType.Char, 1) { Value = req.NumId.Trim() });
        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.VarChar, 16) { Value = (req.UserId ?? string.Empty).Trim() });
        cmd.Parameters.Add(new SqlParameter("@IsMust", SqlDbType.Int) { Value = req.IsMust ?? 0 });
        cmd.Parameters.Add(new SqlParameter("@IsHand", SqlDbType.Int) { Value = req.IsHand ?? 0 });

        try
        {
            var count = await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true, count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MG000006 copy-to-mat failed, SetClass={SetClass}, NumId={NumId}", req.SetClass, req.NumId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
        }
    }

    [HttpGet("dict-widths")]
    public async Task<ActionResult<IEnumerable<DictWidthDto>>> GetDictWidthsAsync()
    {
        var tables = new[] { "MGN_MINdMatClass", "MGNdSetNumMain", "MGNdSetNumSub", "MGNdSetNumSubDtl" };
        var tableList = string.Join(",", tables.Select(t => $"'{t}'"));
        var sql = $@"
SELECT TableName, FieldName, ISNULL(iFieldWidth, 0) AS W, ISNULL(DisplaySize, 0) AS DS
  FROM CURdTableField WITH (NOLOCK)
 WHERE TableName IN ({tableList})";

        var list = new List<DictWidthDto>();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var tableName = rd["TableName"]?.ToString() ?? string.Empty;
            var fieldName = rd["FieldName"]?.ToString() ?? string.Empty;
            var w = rd["W"] as int? ?? Convert.ToInt32(rd["W"]);
            var ds = rd["DS"] as int? ?? Convert.ToInt32(rd["DS"]);

            var width = w > 0 ? w : (ds > 0 ? ds * 10 : 0);
            if (width <= 0) continue;
            list.Add(new DictWidthDto(tableName, fieldName, width));
        }

        return Ok(list);
    }

    private async Task FillLookupNameFallbackAsync(List<IDictionary<string, object?>> rows, params string[] dictCandidates)
    {
        if (rows == null || rows.Count == 0) return;

        try
        {
            static string ToText(object? v) => v == null || v == DBNull.Value ? "" : v.ToString()?.Trim() ?? "";

            var dictFields = await LoadDictFieldsByCandidatesAsync(dictCandidates);
            if (dictFields.Count == 0) return;

            var sourceLookupFields = dictFields
                .Where(f => !string.IsNullOrWhiteSpace(f.FieldName)
                            && !string.IsNullOrWhiteSpace(f.LookupTable)
                            && !string.IsNullOrWhiteSpace(f.LookupKeyField)
                            && !string.IsNullOrWhiteSpace(f.LookupResultField))
                .ToDictionary(f => f.FieldName!, f => f, StringComparer.OrdinalIgnoreCase);

            var rules = new List<(string TargetField, string SourceField, string LookupTable, string LookupKeyField, string LookupNameField)>();

            foreach (var lkField in dictFields)
            {
                var target = lkField.FieldName?.Trim() ?? "";
                if (!target.StartsWith("Lk_", StringComparison.OrdinalIgnoreCase)) continue;
                if (!target.EndsWith("Name", StringComparison.OrdinalIgnoreCase)) continue;
                if (target.Length <= 7) continue;

                var core = target.Substring(3, target.Length - 7).Trim();
                if (string.IsNullOrWhiteSpace(core)) continue;

                var sourceCandidates = new[] { core, core + "Id", core + "Code" };
                CURdTableField? sourceField = null;
                var sourceName = "";
                foreach (var sc in sourceCandidates)
                {
                    if (sourceLookupFields.TryGetValue(sc, out var hit))
                    {
                        sourceField = hit;
                        sourceName = sc;
                        break;
                    }
                }
                if (sourceField == null) continue;

                var resultParts = (sourceField.LookupResultField ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
                if (resultParts.Count == 0) continue;

                var nameField = resultParts.Count >= 2 ? resultParts[1] : resultParts[0];
                if (string.IsNullOrWhiteSpace(nameField)) continue;

                rules.Add((
                    target,
                    sourceName,
                    sourceField.LookupTable!.Trim(),
                    sourceField.LookupKeyField!.Trim(),
                    nameField
                ));
            }

            if (rules.Count == 0) return;

            var lookupCache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in rules)
            {
                var cacheKey = $"{rule.LookupTable}|{rule.LookupKeyField}|{rule.LookupNameField}";
                if (lookupCache.ContainsKey(cacheKey)) continue;
                lookupCache[cacheKey] = await LoadLookupMapAsync(rule.LookupTable, rule.LookupKeyField, rule.LookupNameField);
            }

            foreach (var row in rows)
            {
                foreach (var rule in rules)
                {
                    var current = row.TryGetValue(rule.TargetField, out var currentVal) ? ToText(currentVal) : "";
                    if (!string.IsNullOrWhiteSpace(current)) continue;

                    var sourceKey = row.TryGetValue(rule.SourceField, out var sourceVal) ? ToText(sourceVal) : "";
                    if (string.IsNullOrWhiteSpace(sourceKey)) continue;

                    var cacheKey = $"{rule.LookupTable}|{rule.LookupKeyField}|{rule.LookupNameField}";
                    if (!lookupCache.TryGetValue(cacheKey, out var map) || map.Count == 0) continue;
                    if (!map.TryGetValue(sourceKey, out var displayName) || string.IsNullOrWhiteSpace(displayName)) continue;

                    row[rule.TargetField] = displayName;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MG000006 mat-class: fallback lookup fill failed");
        }
    }

    private async Task<List<CURdTableField>> LoadDictFieldsByCandidatesAsync(IEnumerable<string> candidates)
    {
        foreach (var raw in candidates ?? Enumerable.Empty<string>())
        {
            var table = (raw ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(table)) continue;

            var fields = await _db.CURdTableFields.AsNoTracking()
                .Where(x => x.TableName == table || x.TableName == ("dbo." + table))
                .ToListAsync();
            if (fields.Count > 0) return fields;
        }
        return new List<CURdTableField>();
    }

    private async Task<Dictionary<string, string>> LoadLookupMapAsync(string lookupTable, string keyField, string nameField)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var tableSql = QuoteObjectName(lookupTable);
        var keySql = QuoteIdentifier(keyField);
        var nameSql = QuoteIdentifier(nameField);
        if (tableSql == null || keySql == null || nameSql == null) return map;

        var sql = $"SELECT {keySql} AS [K], {nameSql} AS [N] FROM {tableSql} WITH (NOLOCK)";
        var rows = await QueryListAsync(sql);
        foreach (var row in rows)
        {
            var k = row.TryGetValue("K", out var keyVal) ? keyVal?.ToString()?.Trim() ?? "" : "";
            var n = row.TryGetValue("N", out var nameVal) ? nameVal?.ToString()?.Trim() ?? "" : "";
            if (!string.IsNullOrWhiteSpace(k) && !string.IsNullOrWhiteSpace(n))
                map[k] = n;
        }
        return map;
    }

    private static string? QuoteIdentifier(string value)
    {
        var v = (value ?? string.Empty).Trim().Trim('[', ']');
        if (!Regex.IsMatch(v, @"^[A-Za-z0-9_]+$")) return null;
        return $"[{v}]";
    }

    private static string? QuoteObjectName(string value)
    {
        var v = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(v)) return null;
        var parts = v.Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim().Trim('[', ']'))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
        if (parts.Count == 0) return null;
        if (parts.Any(p => !Regex.IsMatch(p, @"^[A-Za-z0-9_]+$"))) return null;
        return string.Join(".", parts.Select(p => $"[{p}]"));
    }

    private static string FirstLine(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return "資料庫作業失敗";
        var line = message
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        return string.IsNullOrWhiteSpace(line) ? message.Trim() : line;
    }

    private static ContentResult ErrorText(int statusCode, string message)
    {
        return new ContentResult
        {
            StatusCode = statusCode,
            ContentType = "text/plain; charset=utf-8",
            Content = message
        };
    }

    private async Task<List<IDictionary<string, object?>>> QueryListAsync(string sql, params SqlParameter[] parameters)
    {
        var list = new List<IDictionary<string, object?>>();

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        if (parameters != null && parameters.Length > 0)
            cmd.Parameters.AddRange(parameters);

        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rd.FieldCount; i++)
            {
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            }
            list.Add(row);
        }

        return list;
    }
}

