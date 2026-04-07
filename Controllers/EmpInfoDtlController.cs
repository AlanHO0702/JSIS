using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpInfoDtlController : ControllerBase
    {
        private readonly PcbErpContext _ctx;

        public EmpInfoDtlController(PcbErpContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// 員工明細維護（HPSdEmpInfoDtl.dll 對應）
        /// </summary>
        [HttpGet("GetEmpDetail")]
        public async Task<IActionResult> GetEmpDetailAsync(
            [FromQuery] string itemId,
            [FromQuery] string buttonName,
            [FromQuery] string paperNum)
        {
            if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(buttonName) || string.IsNullOrWhiteSpace(paperNum))
                return BadRequest(new { ok = false, error = "itemId、buttonName、paperNum 皆為必填" });

            var cs = _ctx.Database.GetConnectionString()!;
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 1. 讀取 TranPm 參數設定
            var tranParams = await LoadTranParamsAsync(conn, itemId, buttonName);
            if (tranParams.Count == 0)
                return BadRequest(new { ok = false, error = $"找不到 {itemId}/{buttonName} 的 TranPm 設定" });

            // 2. 載入 TableKind → TableName 對應表
            var tableMap = await LoadTableMapAsync(conn, itemId);

            // 3. 解析參數值
            string empId = "";
            string tableName = "";
            foreach (var p in tranParams)
            {
                var value = await ResolveTranParamValueAsync(conn, tableMap, p, paperNum);
                if (p.SeqNum == 1) empId = value;
                else if (p.SeqNum == 2) tableName = value;
            }

            if (string.IsNullOrWhiteSpace(empId))
                return BadRequest(new { ok = false, error = "無法取得 EmpId" });
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { ok = false, error = "無法取得 TableName" });

            // 4. 取得實體表名
            var realTable = await ResolveRealTableNameAsync(conn, tableName) ?? tableName;

            // 5. 讀取欄位定義（含 layout / lookup 資訊）
            var fieldDefs = await LoadFieldDefsAsync(conn, tableName);
            if (fieldDefs.Count == 0)
                return BadRequest(new { ok = false, error = $"找不到 {tableName} 的欄位定義" });

            // 6. 過濾實體表不存在的虛擬欄位（如 SexName、DepartName 等 lookup 顯示欄）
            var actualCols = await LoadActualColumnsAsync(conn, realTable);
            var validDefs = fieldDefs.Where(f => actualCols.Contains(f.FieldName)).ToList();

            // 7. 查詢員工資料
            var dataRow = await LoadEmpDataAsync(conn, realTable, validDefs.Select(f => f.FieldName).ToList(), empId);

            // 8. 解析 lookup 顯示名稱（查詢各 lookup table）
            var lookupNames = await ResolveLookupNamesAsync(conn, validDefs, dataRow);

            // 9. 組裝分頁結構（依 iShowWhere → iLayRow → iLayColumn 排序）
            var pages = validDefs
                .GroupBy(f => f.IShowWhere <= 0 ? 1 : f.IShowWhere)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    page = g.Key,
                    title = $"明細{g.Key}",
                    fields = g.OrderBy(f => f.ILayRow).ThenBy(f => f.ILayColumn).ThenBy(f => f.SerialNum)
                              .Select(f =>
                              {
                                  var rawVal = dataRow.TryGetValue(f.FieldName, out var v) ? v : null;
                                  var rawStr = rawVal?.ToString() ?? "";
                                  lookupNames.TryGetValue(f.FieldName, out var displayVal);
                                  return new
                                  {
                                      fieldName = f.FieldName,
                                      displayLabel = f.DisplayLabel ?? f.FieldName,
                                      value = rawStr,
                                      displayValue = displayVal ?? "", // lookup 顯示名稱
                                      layRow = f.ILayRow,
                                      layColumn = f.ILayColumn,
                                      dataType = f.DataType ?? "",
                                      hasLookup = !string.IsNullOrWhiteSpace(f.LookupTable),
                                      lookupTable = f.LookupTable ?? "",
                                      lookupKey = f.LookupKeyField ?? "",
                                      lookupResult = f.LookupResultField ?? "",
                                      lookupCond1Field = f.LookupCond1Field ?? "",
                                      lookupCond1Source = f.LookupCond1ResultField ?? "",
                                      lookupCond2Field = f.LookupCond2Field ?? "",
                                      lookupCond2Source = f.LookupCond2ResultField ?? "",
                                      comboStyle = f.ComboStyle ?? 0,
                                      items = f.Items ?? ""
                                  };
                              }).ToList()
                }).ToList();

            return Ok(new { ok = true, empId, tableName, realTable, pages });
        }

        // ── 私有型別 ──────────────────────────────────────────────

        private record TranParamDef(int SeqNum, string? TableKind, string? ParamFieldName, int ParamType);

        private record FieldDef(
            string FieldName,
            string? DisplayLabel,
            int IShowWhere,
            int SerialNum,
            int ILayRow,
            int ILayColumn,
            string? DataType,
            int? ComboStyle,
            string? Items,
            string? LookupTable,
            string? LookupKeyField,
            string? LookupResultField,
            string? LookupCond1Field,
            string? LookupCond1ResultField,
            string? LookupCond2Field,
            string? LookupCond2ResultField);

        // ── 私有方法 ──────────────────────────────────────────────

        private static async Task<List<TranParamDef>> LoadTranParamsAsync(SqlConnection conn, string itemId, string buttonName)
        {
            const string sql = @"
SELECT SeqNum, TableKind, ParamFieldName, ISNULL(ParamType,0) AS ParamType
  FROM CURdOCXItmCusBtnTranPm WITH (NOLOCK)
 WHERE ItemId = @itemId AND ButtonName = @btn
 ORDER BY SeqNum;";
            var list = new List<TranParamDef>();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.Parameters.AddWithValue("@btn", buttonName);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new TranParamDef(
                    SeqNum: Convert.ToInt32(rd["SeqNum"] ?? 0),
                    TableKind: rd["TableKind"]?.ToString(),
                    ParamFieldName: rd["ParamFieldName"]?.ToString(),
                    ParamType: Convert.ToInt32(rd["ParamType"] ?? 0)));
            }
            return list;
        }

        private static async Task<Dictionary<string, string>> LoadTableMapAsync(SqlConnection conn, string itemId)
        {
            const string sql = @"
SELECT TableKind, TableName
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE ItemId = @itemId;";
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var kind = rd["TableKind"]?.ToString()?.Trim() ?? "";
                var tbl = rd["TableName"]?.ToString()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(kind) && !string.IsNullOrWhiteSpace(tbl) && !map.ContainsKey(kind))
                    map[kind] = tbl;
            }
            return map;
        }

        private async Task<string> ResolveTranParamValueAsync(
            SqlConnection conn,
            Dictionary<string, string> tableMap,
            TranParamDef param,
            string paperNum)
        {
            return param.ParamType switch
            {
                0 => await ReadFieldByKindAsync(conn, tableMap, param.TableKind, param.ParamFieldName, paperNum),
                1 => param.ParamFieldName ?? "",
                5 => paperNum,
                _ => param.ParamFieldName ?? ""
            };
        }

        private static async Task<string> ReadFieldByKindAsync(
            SqlConnection conn,
            Dictionary<string, string> tableMap,
            string? tableKind,
            string? fieldName,
            string paperNum)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) return "";
            var dictTable = ResolveDictTableName(tableMap, tableKind);
            if (string.IsNullOrWhiteSpace(dictTable)) return "";
            var realTable = await ResolveRealTableNameAsync(conn, dictTable) ?? dictTable;
            var safeTable = QuoteIdentifier(realTable);
            var safeField = QuoteIdentifier(fieldName);
            if (string.IsNullOrWhiteSpace(safeTable) || string.IsNullOrWhiteSpace(safeField)) return "";

            var sql = $"SELECT TOP 1 {safeField} FROM {safeTable} WITH (NOLOCK) WHERE [PaperNum] = @paperNum";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperNum", paperNum);
            var obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj == DBNull.Value ? "" : obj.ToString()!;
        }

        private static async Task<List<FieldDef>> LoadFieldDefsAsync(SqlConnection conn, string tableName)
        {
            const string sql = @"
SELECT FieldName,
       ISNULL(DisplayLabel, FieldName) AS DisplayLabel,
       ISNULL(iShowWhere, 1)   AS iShowWhere,
       ISNULL(SerialNum, 0)    AS SerialNum,
       ISNULL(iLayRow, 0)      AS iLayRow,
       ISNULL(iLayColumn, 0)   AS iLayColumn,
       DataType,
       ComboStyle,
       Items,
       LookupTable,
       LookupKeyField,
       LookupResultField,
       LookupCond1Field,
       LookupCond1ResultField,
       LookupCond2Field,
       LookupCond2ResultField
  FROM CURdTableField WITH (NOLOCK)
 WHERE TableName = @tbl
   AND ISNULL(Visible, 0) = 1
 ORDER BY iShowWhere, iLayRow, iLayColumn, SerialNum;";
            var list = new List<FieldDef>();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", tableName);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new FieldDef(
                    FieldName: rd["FieldName"].ToString()!,
                    DisplayLabel: rd["DisplayLabel"]?.ToString(),
                    IShowWhere: Convert.ToInt32(rd["iShowWhere"] ?? 1),
                    SerialNum: Convert.ToInt32(rd["SerialNum"] ?? 0),
                    ILayRow: Convert.ToInt32(rd["iLayRow"] ?? 0),
                    ILayColumn: Convert.ToInt32(rd["iLayColumn"] ?? 0),
                    DataType: rd["DataType"]?.ToString(),
                    ComboStyle: rd["ComboStyle"] == DBNull.Value ? null : Convert.ToInt32(rd["ComboStyle"]),
                    Items: rd["Items"]?.ToString(),
                    LookupTable: rd["LookupTable"]?.ToString(),
                    LookupKeyField: rd["LookupKeyField"]?.ToString(),
                    LookupResultField: rd["LookupResultField"]?.ToString(),
                    LookupCond1Field: rd["LookupCond1Field"]?.ToString(),
                    LookupCond1ResultField: rd["LookupCond1ResultField"]?.ToString(),
                    LookupCond2Field: rd["LookupCond2Field"]?.ToString(),
                    LookupCond2ResultField: rd["LookupCond2ResultField"]?.ToString()));
            }
            return list;
        }

        private static async Task<HashSet<string>> LoadActualColumnsAsync(SqlConnection conn, string realTable)
        {
            const string sql = @"
SELECT c.name
  FROM sys.columns c
  JOIN sys.objects o ON o.object_id = c.object_id
 WHERE o.name = @tbl AND o.type IN ('U','V');";
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", realTable);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
                set.Add(rd.GetString(0));
            return set;
        }

        private static async Task<Dictionary<string, object?>> LoadEmpDataAsync(
            SqlConnection conn, string realTable, List<string> fieldNames, string empId)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (fieldNames.Count == 0) return result;

            var safeTable = QuoteIdentifier(realTable);
            if (string.IsNullOrWhiteSpace(safeTable)) return result;

            var cols = string.Join(", ", fieldNames
                .Select(QuoteIdentifier)
                .Where(c => !string.IsNullOrWhiteSpace(c)));
            if (string.IsNullOrWhiteSpace(cols)) return result;

            var sql = $"SELECT TOP 1 {cols} FROM {safeTable} WITH (NOLOCK) WHERE [EmpId] = @empId";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@empId", empId);
            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                for (int i = 0; i < rd.FieldCount; i++)
                    result[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            }
            return result;
        }

        /// <summary>
        /// 針對有 LookupTable 的欄位，查詢 lookup 顯示名稱（支援多結果欄位與 cond1/cond2）
        /// </summary>
        private static async Task<Dictionary<string, string>> ResolveLookupNamesAsync(
            SqlConnection conn,
            List<FieldDef> defs,
            Dictionary<string, object?> dataRow)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var tableNameCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var displayCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in defs)
            {
                if (string.IsNullOrWhiteSpace(field.LookupTable)
                    || string.IsNullOrWhiteSpace(field.LookupKeyField)
                    || string.IsNullOrWhiteSpace(field.LookupResultField))
                    continue;

                var code = dataRow.TryGetValue(field.FieldName, out var obj)
                    ? (obj?.ToString() ?? "").Trim()
                    : "";
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                var lookupTable = field.LookupTable!.Trim();
                if (!tableNameCache.TryGetValue(lookupTable, out var actualLookupTable))
                {
                    actualLookupTable = await ResolveRealTableNameAsync(conn, lookupTable) ?? lookupTable;
                    tableNameCache[lookupTable] = actualLookupTable;
                }

                var keyFields = SplitColumns(field.LookupKeyField);
                var resultFields = SplitColumns(field.LookupResultField);
                if (keyFields.Length == 0 || resultFields.Length == 0)
                    continue;

                var cond1Field = (field.LookupCond1Field ?? "").Trim();
                var cond1Source = (field.LookupCond1ResultField ?? "").Trim();
                var cond2Field = (field.LookupCond2Field ?? "").Trim();
                var cond2Source = (field.LookupCond2ResultField ?? "").Trim();
                var cond1Value = ReadLookupCondValue(dataRow, cond1Source, cond1Field);
                var cond2Value = ReadLookupCondValue(dataRow, cond2Source, cond2Field);

                var cacheKey =
                    $"{actualLookupTable}|{string.Join(",", keyFields)}|{code}|{string.Join(",", resultFields)}|{cond1Field}:{cond1Value}|{cond2Field}:{cond2Value}";
                if (displayCache.TryGetValue(cacheKey, out var cachedDisplay))
                {
                    if (!string.IsNullOrWhiteSpace(cachedDisplay))
                        result[field.FieldName] = cachedDisplay;
                    continue;
                }

                var safeTable = QuoteIdentifier(actualLookupTable);
                if (string.IsNullOrWhiteSpace(safeTable))
                    continue;

                var whereParts = new List<string>();
                var selectParts = new List<string>();
                var parameters = new List<(string Name, string Value)>();
                var valid = true;

                var keyValues = SplitCompositeKey(code, keyFields.Length);
                for (int i = 0; i < keyFields.Length; i++)
                {
                    var safeKey = QuoteIdentifier(keyFields[i]);
                    if (string.IsNullOrWhiteSpace(safeKey))
                    {
                        valid = false;
                        break;
                    }
                    var p = $"@k{i}";
                    whereParts.Add($"{safeKey} = {p}");
                    parameters.Add((p, keyValues[i]));
                }

                if (!valid || whereParts.Count == 0)
                    continue;

                for (int i = 0; i < resultFields.Length; i++)
                {
                    var safeResult = QuoteIdentifier(resultFields[i]);
                    if (string.IsNullOrWhiteSpace(safeResult))
                    {
                        valid = false;
                        break;
                    }
                    selectParts.Add($"{safeResult} AS [r{i}]");
                }

                if (!valid || selectParts.Count == 0)
                    continue;

                if (!string.IsNullOrWhiteSpace(cond1Field) && !string.IsNullOrWhiteSpace(cond1Value))
                {
                    var safeCond1 = QuoteIdentifier(cond1Field);
                    if (string.IsNullOrWhiteSpace(safeCond1)) continue;
                    whereParts.Add($"{safeCond1} = @c1");
                    parameters.Add(("@c1", cond1Value));
                }

                if (!string.IsNullOrWhiteSpace(cond2Field) && !string.IsNullOrWhiteSpace(cond2Value))
                {
                    var safeCond2 = QuoteIdentifier(cond2Field);
                    if (string.IsNullOrWhiteSpace(safeCond2)) continue;
                    whereParts.Add($"{safeCond2} = @c2");
                    parameters.Add(("@c2", cond2Value));
                }

                var sql = $"SELECT TOP 1 {string.Join(", ", selectParts)} FROM {safeTable} WITH (NOLOCK) WHERE {string.Join(" AND ", whereParts)}";
                var displayText = "";
                try
                {
                    await using var cmd = new SqlCommand(sql, conn);
                    foreach (var p in parameters)
                        cmd.Parameters.AddWithValue(p.Name, p.Value ?? "");

                    await using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        var chunks = new List<string>();
                        for (int i = 0; i < resultFields.Length; i++)
                        {
                            var val = rd.IsDBNull(i) ? "" : (rd.GetValue(i)?.ToString() ?? "").Trim();
                            if (!string.IsNullOrWhiteSpace(val))
                                chunks.Add(val);
                        }
                        displayText = string.Join(" - ", chunks);
                    }
                }
                catch
                {
                    displayText = "";
                }

                displayCache[cacheKey] = displayText;
                if (!string.IsNullOrWhiteSpace(displayText))
                    result[field.FieldName] = displayText;
            }

            return result;
        }

        private static string[] SplitColumns(string? csv) =>
            (csv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

        private static string[] SplitCompositeKey(string rawKey, int keyCount)
        {
            if (keyCount <= 1) return new[] { rawKey ?? "" };
            var parts = (rawKey ?? "").Split('\u001F');
            if (parts.Length == keyCount) return parts;

            var arr = Enumerable.Repeat("", keyCount).ToArray();
            if (parts.Length == 1)
            {
                arr[0] = rawKey ?? "";
                return arr;
            }

            for (int i = 0; i < Math.Min(parts.Length, keyCount); i++)
                arr[i] = parts[i] ?? "";

            return arr;
        }

        private static string ReadLookupCondValue(
            Dictionary<string, object?> dataRow,
            string sourceField,
            string fallbackField)
        {
            var source = (sourceField ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(source) && dataRow.TryGetValue(source, out var srcVal))
                return srcVal?.ToString()?.Trim() ?? "";

            var fallback = (fallbackField ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(fallback) && dataRow.TryGetValue(fallback, out var fbVal))
                return fbVal?.ToString()?.Trim() ?? "";

            return "";
        }

        private static async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, string dictTable)
        {
            const string sql = @"
SELECT TOP 1 ISNULL(NULLIF(RealTableName,''), TableName) AS ActualName
  FROM CURdTableName WITH (NOLOCK)
 WHERE TableName = @tbl;";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTable ?? "");
            var obj = await cmd.ExecuteScalarAsync();
            return obj == null || obj == DBNull.Value ? null : obj.ToString();
        }

        private static string? ResolveDictTableName(Dictionary<string, string> tableMap, string? tableKind)
        {
            var kind = (tableKind ?? "").Trim();
            if (string.IsNullOrWhiteSpace(kind)) return null;
            if (tableMap.TryGetValue(kind, out var name)) return name;
            var master = tableMap.FirstOrDefault(x => x.Key.Contains("Master", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(master.Value) ? null : master.Value;
        }

        private static bool IsValidIdentifierPart(string part) =>
            Regex.IsMatch(part, @"^[A-Za-z_][A-Za-z0-9_]*$");

        private static string QuoteIdentifier(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var n = name.Trim().Trim('[', ']');
            var parts = n.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Any(p => !IsValidIdentifierPart(p))) return "";
            return string.Join(".", parts.Select(p => $"[{p}]"));
        }
    }
}
