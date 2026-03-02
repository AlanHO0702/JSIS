// Controllers/ReportController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly PcbErpContext _context;
        private readonly IHttpClientFactory _httpFactory;
        public ReportController(IConfiguration config, PcbErpContext context, IHttpClientFactory httpFactory)
        {
            _config = config;
            _context = context;
            _httpFactory = httpFactory;
        }

        [HttpPost("generate-url")]
        public async Task<IActionResult> GenerateUrl([FromBody] BuildRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.SpName))
                return BadRequest(new { error = "缺少 SpName" });

            // 1) 如需先跑 SP
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = req.SpName;
                if (req.Params != null)
                    foreach (var kv in req.Params) cmd.Parameters.Add(new SqlParameter("@" + kv.Key, ToDbValue(kv.Value)));
                else if (!string.IsNullOrWhiteSpace(req.PaperNum))
                    cmd.Parameters.Add(new SqlParameter("@PaperNum", ToDbValue(req.PaperNum)));

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            // 2) POST 到 CrystalReportsAPI，拿 PDF blob 回來
            var renderPayload = new {
                reportName = req.ReportName,
                format = "pdf",
                @params = req.Params ?? new Dictionary<string, object>()
            };
            if (!renderPayload.@params.ContainsKey("PaperNum") && !string.IsNullOrWhiteSpace(req.PaperNum))
                renderPayload.@params["PaperNum"] = req.PaperNum;

            var baseUrl = _config["ReportApi:CrystalUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                return StatusCode(500, "ReportApi:CrystalUrl is not configured.");
            var http = _httpFactory.CreateClient("CrystalReport");
            var resp = await http.PostAsJsonAsync("/api/report/render", renderPayload);
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, await resp.Content.ReadAsStringAsync());

            var pdfBytes = await resp.Content.ReadAsByteArrayAsync();
            return File(pdfBytes, "application/pdf"); // 前端拿到就是 PDF 流
        }

        [HttpGet("paper-options")]
        public async Task<IActionResult> GetPaperOptions([FromQuery] string paperId)
        {
            if (string.IsNullOrWhiteSpace(paperId))
                return BadRequest(new { ok = false, error = "缺少 PaperId" });

            var list = new List<object>();
            var connStr = _config.GetConnectionString("DefaultConnection");
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"
SELECT SerialNum, ItemName, Enabled, ClassName, ObjectName
  FROM CURdPaperPaper WITH (NOLOCK)
 WHERE PaperId = @paperId
 ORDER BY SerialNum;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@paperId", paperId);

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var enabled = rd["Enabled"] == DBNull.Value ? 0 : Convert.ToInt32(rd["Enabled"]);
                if (enabled != 1) continue;

                var itemName = rd["ItemName"]?.ToString() ?? string.Empty;
                var className = rd["ClassName"]?.ToString() ?? string.Empty;
                var objectName = rd["ObjectName"]?.ToString() ?? string.Empty;

                var reportName = className.EndsWith(".rpt", StringComparison.OrdinalIgnoreCase)
                    ? className[..^4]
                    : className;
                if (string.IsNullOrWhiteSpace(reportName))
                    reportName = objectName;

                var spName = string.IsNullOrWhiteSpace(objectName)
                    ? reportName
                    : objectName;

                list.Add(new
                {
                    ItemName = itemName,
                    SpName = spName,
                    ReportName = reportName
                });
            }

            return Ok(new { ok = true, list });
        }


        public class BuildRequest
        {
            public string PaperNum { get; set; } = "";
            public string SessionId { get; set; } = "";
            public string SpName { get; set; } = "";
            public string ReportName { get; set; } = "";
            public Dictionary<string, object>? Params { get; set; }   // 新增
        }


        public class ReportExecRequest
        {
            public string SpName { get; set; } = string.Empty;
            public Dictionary<string, object>? Params { get; set; }
        }

        // ====== 通用：執行 SP，回傳動態欄位與資料 ======
        [HttpPost("exec")]
        public async Task<IActionResult> ExecSP([FromBody] ReportExecRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.SpName))
                return BadRequest("必須指定 SpName");

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(req.SpName, conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 180
            };

            // 將前端參數安全轉型後再加入（解決 JsonElement 無法對應問題）
            if (req.Params is not null)
            {
                foreach (var kv in req.Params)
                {
                    var value = ToDbValue(kv.Value);
                    var p = new SqlParameter("@" + kv.Key, value ?? DBNull.Value);
                    cmd.Parameters.Add(p);
                }
            }

            var dt = new DataTable();
            using (var da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }

            // 轉成容易序列化的物件（把 DBNull 換成 null）
            var columns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            var rows = dt.Rows.Cast<DataRow>()
                .Select(r => dt.Columns.Cast<DataColumn>()
                    .ToDictionary(c => c.ColumnName, c =>
                        r[c] == DBNull.Value ? null : r[c]))
                .ToList();

            return Ok(new { Columns = columns, Rows = rows });
        }

        // ====== 通用：下拉查詢（白名單） ======
        // ====== 查詢項目 Meta (含 ItemType/OutputType) ======
        [HttpGet("item-meta/{itemId}")]
        public async Task<IActionResult> GetItemMeta(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return BadRequest("itemId required");

            var item = await _context.CurdSysItems
                .AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .Select(x => new { x.ItemId, x.ItemName, x.ItemType, x.OutputType, x.Ocxtemplete })
                .SingleOrDefaultAsync();

            if (item is null) return NotFound();
            return Ok(item);
        }

        [HttpGet("lookup/{key}")]
        public async Task<IActionResult> Lookup(string key)
        {
            key = key ?? string.Empty;
            var parentVal = HttpContext.Request.Query["parent"].FirstOrDefault();
            if (key.StartsWith("db:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = key.Split(':', 3);
                if (parts.Length < 3) return BadRequest("invalid lookup key");
                var itemId = parts[1];
                var param = parts[2];

                var p = await _context.CurdAddonParams
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.ItemId == itemId && x.ParamName == "@" + param);

                if (p == null || string.IsNullOrWhiteSpace(p.CommandText))
                    return NotFound();

                var dbSql = p.CommandText.Trim();
                if (!dbSql.StartsWith("select", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("only SELECT is allowed");

                var dbList = await RunLookupQuery(dbSql, parentVal);
                return Ok(dbList);
            }

            var sql = (key ?? string.Empty).ToLowerInvariant() switch
                    {
                        "customer" => "select CompanyID as value, ShortName as text from AJNdCustomer(nolock) order by CompanyID",
                        "bu" => "select rtrim(ltrim(BUId)) as value, BUName as text from CURdBU(nolock)",
                        "shipterm" => "select ShipTerm as value, ShipTermName as text from SPOdShipTerm(nolock) union select 255,'不限'",
                        "finished" => "select finished as value, finishedname as text from CURdPaperFinished(nolock) union select 5,'已完成及已結案' union select 255,'不設限'",
                       
                        _ => throw new ArgumentException("lookup key not allowed")
                    };

            var list = await RunLookupQuery(sql, parentVal);
            return Ok(list);
        }

        // ====== Pivot 自訂格式（DB）======
        [HttpGet("pivot-presets")]
        public async Task<IActionResult> GetPivotPresets([FromQuery] string itemId, [FromQuery] string? userId = null)
        {
            itemId = (itemId ?? string.Empty).Trim();
            userId = ResolveUserId(userId);
            if (string.IsNullOrWhiteSpace(itemId))
                return BadRequest(new { ok = false, error = "itemId required" });

            var connStr = _config.GetConnectionString("DefaultConnection");
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            var list = new List<PivotPresetDto>();

            const string sqlSubNames = @"
;WITH cte AS (
    SELECT
        SubName,
        UserId,
        ROW_NUMBER() OVER (
            PARTITION BY SubName
            ORDER BY CASE WHEN UserId = @userId THEN 0 ELSE 1 END, UserId
        ) AS rn
    FROM CURdSysItemsUser WITH (NOLOCK)
    WHERE ItemId = @itemId
      AND (UserId = @userId OR UserId = '')
      AND ISNULL(SubName, '') <> ''
)
SELECT SubName, UserId
FROM cte
WHERE rn = 1
ORDER BY SubName;";

            await using (var cmd = new SqlCommand(sqlSubNames, conn))
            {
                cmd.Parameters.AddWithValue("@itemId", itemId);
                cmd.Parameters.AddWithValue("@userId", userId);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var subName = rd["SubName"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(subName)) continue;
                    var owner = rd["UserId"]?.ToString() ?? string.Empty;
                    list.Add(new PivotPresetDto
                    {
                        Name = subName,
                        SourceUserId = owner,
                        IsShared = string.IsNullOrWhiteSpace(owner)
                    });
                }
            }

            const string sqlFields = @"
SELECT FieldName, AreaId, AreaIndex
FROM CURdSysItemsUserField WITH (NOLOCK)
WHERE ItemId = @itemId
  AND UserId = @ownerUserId
  AND SubName = @subName
ORDER BY AreaId, AreaIndex;";

            foreach (var preset in list)
            {
                await using var cmd = new SqlCommand(sqlFields, conn);
                cmd.Parameters.AddWithValue("@itemId", itemId);
                cmd.Parameters.AddWithValue("@ownerUserId", preset.SourceUserId ?? string.Empty);
                cmd.Parameters.AddWithValue("@subName", preset.Name ?? string.Empty);

                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var fieldName = rd["FieldName"]?.ToString() ?? string.Empty;
                    var areaId = rd["AreaId"] == DBNull.Value ? 0 : Convert.ToInt32(rd["AreaId"]);
                    if (string.IsNullOrWhiteSpace(fieldName)) continue;

                    switch (areaId)
                    {
                        case 1:
                            preset.Rows.Add(fieldName);
                            break;
                        case 2:
                            preset.Cols.Add(fieldName);
                            break;
                        case 3:
                            preset.Vals.Add(fieldName);
                            break;
                    }
                }
            }

            return Ok(new { ok = true, itemId, userId, presets = list });
        }

        [HttpPost("pivot-presets/save")]
        public async Task<IActionResult> SavePivotPreset([FromBody] PivotPresetSaveRequest req)
        {
            req ??= new PivotPresetSaveRequest();
            var itemId = (req.ItemId ?? string.Empty).Trim();
            var name = (req.Name ?? string.Empty).Trim();
            var userId = ResolveUserId(req.UserId);

            if (string.IsNullOrWhiteSpace(itemId))
                return BadRequest(new { ok = false, error = "itemId required" });
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { ok = false, error = "name required" });

            var rows = NormalizeFields(req.Rows);
            var cols = NormalizeFields(req.Cols);
            var vals = NormalizeFields(req.Vals);

            var connStr = _config.GetConnectionString("DefaultConnection");
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                // 只覆蓋目前使用者版本，避免誤刪共用或他人版本
                await ExecNonQueryAsync(conn, tx, @"
DELETE FROM CURdSysItemsUserField
WHERE ItemId = @itemId AND UserId = @userId AND SubName = @subName;", itemId, userId, name);

                await ExecNonQueryAsync(conn, tx, @"
DELETE FROM CURdSysItemsUser
WHERE ItemId = @itemId AND UserId = @userId AND SubName = @subName;", itemId, userId, name);

                await using (var cmdInsHead = new SqlCommand(@"
INSERT INTO CURdSysItemsUser(ItemId, UserId, SubName)
VALUES(@itemId, @userId, @subName);", conn, (SqlTransaction)tx))
                {
                    cmdInsHead.Parameters.AddWithValue("@itemId", itemId);
                    cmdInsHead.Parameters.AddWithValue("@userId", userId);
                    cmdInsHead.Parameters.AddWithValue("@subName", name);
                    await cmdInsHead.ExecuteNonQueryAsync();
                }

                await InsertFieldRowsAsync(conn, tx, itemId, userId, name, rows, 1);
                await InsertFieldRowsAsync(conn, tx, itemId, userId, name, cols, 2);
                await InsertFieldRowsAsync(conn, tx, itemId, userId, name, vals, 3);

                await tx.CommitAsync();
                return Ok(new { ok = true, itemId, userId, name });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }

        [HttpDelete("pivot-presets")]
        public async Task<IActionResult> DeletePivotPreset([FromQuery] string itemId, [FromQuery] string name, [FromQuery] string? userId = null)
        {
            itemId = (itemId ?? string.Empty).Trim();
            name = (name ?? string.Empty).Trim();
            userId = ResolveUserId(userId);

            if (string.IsNullOrWhiteSpace(itemId))
                return BadRequest(new { ok = false, error = "itemId required" });
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { ok = false, error = "name required" });

            var connStr = _config.GetConnectionString("DefaultConnection");
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                await ExecNonQueryAsync(conn, tx, @"
DELETE FROM CURdSysItemsUserField
WHERE ItemId = @itemId AND UserId = @userId AND SubName = @subName;", itemId, userId, name);

                var affected = await ExecNonQueryAsync(conn, tx, @"
DELETE FROM CURdSysItemsUser
WHERE ItemId = @itemId AND UserId = @userId AND SubName = @subName;", itemId, userId, name);

                await tx.CommitAsync();
                return Ok(new { ok = true, itemId, userId, name, affected });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }

        private async Task<List<object>> RunLookupQuery(string sql, string? parentVal = null)
        {
            await using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            if (sql.Contains("@@@@@"))
            {
                sql = sql.Replace("@@@@@", "@p0");
                var p = cmd.CreateParameter();
                p.ParameterName = "@p0";
                p.Value = parentVal ?? string.Empty;
                cmd.Parameters.Add(p);
            }

            cmd.CommandText = sql;
            var list = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();

            // 找出 value / text 欄位，若沒別名則退而求其次取前兩欄
            var ordValue = -1;
            var ordText = -1;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (ordValue == -1 && string.Equals(name, "value", StringComparison.OrdinalIgnoreCase))
                    ordValue = i;
                if (ordText == -1 && string.Equals(name, "text", StringComparison.OrdinalIgnoreCase))
                    ordText = i;
            }
            if (ordValue == -1 && reader.FieldCount > 0) ordValue = 0;
            if (ordText == -1 && reader.FieldCount > 1) ordText = 1;
            if (ordText == -1) ordText = ordValue; // 只有一欄時，text 跟 value 同欄
            if (ordValue == -1) return list;       // 沒有任何欄位就回傳空

            while (await reader.ReadAsync())
            {
                var value = reader.IsDBNull(ordValue) ? "" : reader.GetValue(ordValue)?.ToString() ?? "";
                var text = reader.IsDBNull(ordText) ? "" : reader.GetValue(ordText)?.ToString() ?? "";
                list.Add(new { value, text });
            }
            return list;
        }

        private static object? ToDbValue(object? v)
        {
            if (v is null) return DBNull.Value;

            // 1) 字串：保留原值（包含空字串），不要轉 DBNull
            if (v is string s)
                return s;

            // 2) 前端 JSON 來的值通常是 JsonElement：交給 FromJson
            if (v is System.Text.Json.JsonElement je) 
                return FromJson(je);

            return v;
        }

        private static object? FromJson(System.Text.Json.JsonElement je)
        {
            switch (je.ValueKind)
            {
                case System.Text.Json.JsonValueKind.Null:
                case System.Text.Json.JsonValueKind.Undefined:
                    return DBNull.Value;

                case System.Text.Json.JsonValueKind.String:
                {
                    var s = je.GetString() ?? string.Empty;        // ← 保留空字串
                    // 日期欄位前端已經送 null（不是空字串），所以這裡只要能 parse 再轉 DateTime
                    if (!string.IsNullOrEmpty(s) && DateTime.TryParse(s, out var dt))
                        return dt;
                    return s; // 其餘當成 nvarchar 傳入（包括空字串）
                }

                case System.Text.Json.JsonValueKind.Number:
                    if (je.TryGetInt32(out var i32)) return i32;
                    if (je.TryGetInt64(out var i64)) return i64;
                    if (je.TryGetDecimal(out var dec)) return dec;
                    return je.GetDouble();

                case System.Text.Json.JsonValueKind.True:  return true;
                case System.Text.Json.JsonValueKind.False: return false;

                // 物件/陣列等不支援型別，一律轉字串
                default:
                    return je.ToString();
            }
        }

        private string ResolveUserId(string? userId)
        {
            var v = (userId ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(v)) return v;

            v = (HttpContext?.Items["UserId"]?.ToString() ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(v)) return v;

            v = (User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UserId", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(v) ? "admin" : v;
        }

        private static List<string> NormalizeFields(IEnumerable<string>? source)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<string>();
            if (source == null) return list;

            foreach (var s in source)
            {
                var v = (s ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(v)) continue;
                if (v.Length > 200) v = v[..200];
                if (set.Add(v)) list.Add(v);
            }
            return list;
        }

        private static async Task<int> ExecNonQueryAsync(
            SqlConnection conn,
            DbTransaction tx,
            string sql,
            string itemId,
            string userId,
            string subName)
        {
            await using var cmd = new SqlCommand(sql, conn, (SqlTransaction)tx);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@subName", subName);
            return await cmd.ExecuteNonQueryAsync();
        }

        private static async Task InsertFieldRowsAsync(
            SqlConnection conn,
            DbTransaction tx,
            string itemId,
            string userId,
            string subName,
            IReadOnlyList<string> fields,
            int areaId)
        {
            if (fields.Count == 0) return;

            const string sql = @"
INSERT INTO CURdSysItemsUserField(ItemId, UserId, SubName, FieldName, AreaId, AreaIndex)
VALUES(@itemId, @userId, @subName, @fieldName, @areaId, @areaIndex);";

            for (var i = 0; i < fields.Count; i++)
            {
                await using var cmd = new SqlCommand(sql, conn, (SqlTransaction)tx);
                cmd.Parameters.AddWithValue("@itemId", itemId);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@subName", subName);
                cmd.Parameters.AddWithValue("@fieldName", fields[i]);
                cmd.Parameters.AddWithValue("@areaId", areaId);
                cmd.Parameters.AddWithValue("@areaIndex", i);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public class PivotPresetDto
        {
            public string Name { get; set; } = string.Empty;
            public bool IsShared { get; set; }
            public string SourceUserId { get; set; } = string.Empty;
            public List<string> Rows { get; set; } = new();
            public List<string> Cols { get; set; } = new();
            public List<string> Vals { get; set; } = new();
        }

        public class PivotPresetSaveRequest
        {
            public string ItemId { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public List<string> Rows { get; set; } = new();
            public List<string> Cols { get; set; } = new();
            public List<string> Vals { get; set; } = new();
        }

    }
}
