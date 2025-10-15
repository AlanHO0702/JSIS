// Controllers/ReportController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ReportController(IConfiguration config) => _config = config;

        // ====== Crystal 報表外掛：若你有用就保留，沒有可以刪掉 ======
        [HttpPost("generate-url")]
        public IActionResult GenerateUrl([FromBody] BuildRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.SpName))
                    return BadRequest(new { error = "缺少 SpName 參數" });

                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connStr);
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = req.SpName;
                cmd.Parameters.Add(new SqlParameter("@PaperNum", ToDbValue(req.PaperNum)));

                conn.Open();
                cmd.ExecuteNonQuery();

                var reportUrl =
                    $"{_config["ReportApi:CrystalUrl"]}/api/report/render?reportName={Uri.EscapeDataString(req.ReportName)}&paperNum={Uri.EscapeDataString(req.PaperNum)}&format=pdf";

                return Ok(new { reportUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class BuildRequest
        {
            public string PaperNum { get; set; } = string.Empty;
            public string SessionId { get; set; } = string.Empty;
            public string SpName { get; set; } = string.Empty;
            public string ReportName { get; set; } = string.Empty;
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
        [HttpGet("lookup/{key}")]
        public async Task<IActionResult> Lookup(string key)
        {
            var sql = (key ?? string.Empty).ToLowerInvariant() switch
            {
                "customer" => "select CompanyID as value, ShortName as text from AJNdCustomer(nolock) order by CompanyID",
                "bu"       => "select BUId as value, BUName as text from CURdBU(nolock)",
                "shipterm" => "select ShipTerm as value, ShipTermName as text from SPOdShipTerm(nolock) union select 255,'不限'",
                _ => throw new ArgumentException("lookup key not allowed")
            };

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            using var rd = await cmd.ExecuteReaderAsync();

            var list = new List<object>();
            while (await rd.ReadAsync())
            {
                var value = rd["value"] is DBNull ? null : rd["value"];
                var text  = rd["text"]  is DBNull ? null : rd["text"];
                list.Add(new { value, text });
            }

            return Ok(list);
        }

        // ====== 參數轉型 Helper（重點！） ======
        // ====== 參數轉型 Helper（修正版） ======
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

    }
}
