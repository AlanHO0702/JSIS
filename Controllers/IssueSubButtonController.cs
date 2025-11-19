// Controllers/IssueSubButtonController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json.Serialization;

[ApiController]
[Route("api/[controller]")]
public class IssueSubButtonController : ControllerBase
{
    private readonly string _cs;
    public IssueSubButtonController(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // 工具：null → DBNull
    static object Db(object v) => v is null ? DBNull.Value : v;

    // ① 查詢：FMEdIssuePOChoice（製令單用的訂單明細搜尋）
    [HttpPost("fetch")]
    public async Task<IActionResult> Fetch([FromBody] FetchReq req)
    {
        // Debug: 記錄收到的參數
        Console.WriteLine($"[DEBUG] Fetch called with: PONum={req.PONum}, PartNum={req.PartNum}, PaperNum={req.PaperNum}, LessInq={req.LessInq}, HasIssue={req.HasIssue}, POType={req.POType}");

        using var conn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("dbo.FMEdIssuePOChoice", conn)
        { CommandType = CommandType.StoredProcedure };

        // 空→空字串
        string S(string? v) => (v ?? string.Empty).Trim();

        // 主要查詢參數
        cmd.Parameters.Add(new SqlParameter("@PONum", SqlDbType.VarChar, 20) { Value = S(req.PONum) });
        cmd.Parameters.Add(new SqlParameter("@SerialNum", SqlDbType.Int) { Value = req.SerialNum ?? 255 });
        cmd.Parameters.Add(new SqlParameter("@Partnum", SqlDbType.NVarChar, 120) { Value = S(req.PartNum) });
        cmd.Parameters.Add(new SqlParameter("@Revision", SqlDbType.NVarChar, 20) { Value = S(req.Revision) });
        cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.VarChar, 16) { Value = S(req.CustomerId) });
        cmd.Parameters.Add(new SqlParameter("@Merge", SqlDbType.Int) { Value = 0 });
        cmd.Parameters.Add(new SqlParameter("@MPartnum", SqlDbType.NVarChar, 120) { Value = "" });
        cmd.Parameters.Add(new SqlParameter("@MRevision", SqlDbType.NVarChar, 20) { Value = "" });
        cmd.Parameters.Add(new SqlParameter("@HasIssue", SqlDbType.Int) { Value = req.HasIssue ?? 0 }); // 0:未發, 1:已發, 255:全部
        cmd.Parameters.Add(new SqlParameter("@UseId", SqlDbType.VarChar, 20) { Value = "A001" });
        cmd.Parameters.Add(new SqlParameter("@PaperNum", SqlDbType.VarChar, 16) { Value = S(req.PaperNum) }); // 製令單號
        cmd.Parameters.Add(new SqlParameter("@POType", SqlDbType.Int) { Value = req.POType ?? 255 });
        cmd.Parameters.Add(new SqlParameter("@SourCustomerId", SqlDbType.VarChar, 16) { Value = "" });
        cmd.Parameters.Add(new SqlParameter("@EMO_Status", SqlDbType.Int) { Value = 255 });
        cmd.Parameters.Add(new SqlParameter("@StatusChk4", SqlDbType.Int) { Value = 255 });
        cmd.Parameters.Add(new SqlParameter("@LessInq", SqlDbType.Int) { Value = req.LessInq ?? 255 }); // 缺料查詢: 0/1/255
        cmd.Parameters.Add(new SqlParameter("@HDI", SqlDbType.VarChar, 24) { Value = "" });

        await conn.OpenAsync();
        using var rdr = await cmd.ExecuteReaderAsync();

        var rows = new List<Dictionary<string, object?>>();
        while (await rdr.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rdr.FieldCount; i++)
                row[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
            rows.Add(row);
        }

        Console.WriteLine($"[DEBUG] SP returned {rows.Count} rows");
        return Ok(new { ok = true, data = rows });
    }

    // ② 匯入：FMEdIssuePOInsertPCB（將訂單明細匯入到製令單明細）
    [HttpPost("insert")]
    public async Task<IActionResult> Insert([FromBody] InsertReq req)
    {
        Console.WriteLine($"[INSERT] 開始匯入，目的單號: {req.DllPaperNum}, 資料筆數: {req.Rows?.Count ?? 0}");

        if (string.IsNullOrWhiteSpace(req.DllPaperNum))
            return BadRequest("缺少目的單號 DllPaperNum。");
        if (req.Rows is null || req.Rows.Count == 0)
            return BadRequest("沒有要匯入的資料。");

        using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        int qCnt = req.Rows.Count;
        int successCount = 0;

        foreach (var r in req.Rows)
        {
            Console.WriteLine($"[INSERT] 處理第 {successCount + 1} 筆: PONum={r.PONum}, SerialNum={r.SerialNum}, MasPartNum={r.MasPartNum}, MasRevision={r.MasRevision}");

            using var cmd = new SqlCommand("dbo.FMEdIssuePOInsertPCB", conn)
            { CommandType = CommandType.StoredProcedure };

            // SP 必要參數
            cmd.Parameters.Add(new SqlParameter("@DLLPaperNum", SqlDbType.VarChar, 16) { Value = req.DllPaperNum });
            cmd.Parameters.Add(new SqlParameter("@iFlag", SqlDbType.Int) { Value = 1 });
            cmd.Parameters.Add(new SqlParameter("@PONum", SqlDbType.VarChar, 16) { Value = Db(r.PONum) });
            cmd.Parameters.Add(new SqlParameter("@SerialNum", SqlDbType.Int) { Value = r.SerialNum });
            cmd.Parameters.Add(new SqlParameter("@POType", SqlDbType.Int) { Value = r.POType });
            cmd.Parameters.Add(new SqlParameter("@MasPartNum", SqlDbType.VarChar, 120) { Value = Db(r.MasPartNum) });
            cmd.Parameters.Add(new SqlParameter("@MasRevision", SqlDbType.VarChar, 20) { Value = Db(r.MasRevision) });

            // 缺料查詢相關參數
            cmd.Parameters.Add(new SqlParameter("@LessInq", SqlDbType.Int) { Value = req.LessInq ? 1 : 0 });
            cmd.Parameters.Add(new SqlParameter("@StockQnty", SqlDbType.NVarChar, 24) { Value = r.StockQnty?.ToString() ?? "" });
            cmd.Parameters.Add(new SqlParameter("@WIP", SqlDbType.NVarChar, 24) { Value = r.WIP?.ToString() ?? "" });
            cmd.Parameters.Add(new SqlParameter("@AllOutQnty", SqlDbType.NVarChar, 24) { Value = r.AllOutQnty?.ToString() ?? "" });
            cmd.Parameters.Add(new SqlParameter("@NeedQnty", SqlDbType.NVarChar, 24) { Value = r.NeedQnty?.ToString() ?? "" });
            cmd.Parameters.Add(new SqlParameter("@IssQnty", SqlDbType.NVarChar, 24) { Value = r.IssQnty?.ToString() ?? "" });
            cmd.Parameters.Add(new SqlParameter("@UnIssQnty", SqlDbType.NVarChar, 24) { Value = r.UnIssQnty?.ToString() ?? "" });
            cmd.Parameters.Add(new SqlParameter("@POKind", SqlDbType.Int) { Value = r.POKind });

            try
            {
                var affectedRows = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[INSERT] 第 {successCount + 1} 筆 SP 執行完成，影響列數: {affectedRows}");
                successCount++;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"[INSERT ERROR] SQL 錯誤 - 第 {successCount + 1} 筆執行失敗: {sqlEx.Message}");
                Console.WriteLine($"[INSERT ERROR] SQL Error Number: {sqlEx.Number}");

                // 返回友善的錯誤訊息
                string errorMsg = sqlEx.Number switch
                {
                    2627 => "違反唯一性條件約束：此訂單明細可能已經匯入過了",
                    2601 => "重複的索引鍵：此訂單明細可能已經匯入過了",
                    547 => "違反外鍵約束：參考的資料不存在",
                    _ => $"資料庫錯誤：{sqlEx.Message}"
                };

                return BadRequest(new { ok = false, error = errorMsg, detail = sqlEx.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INSERT ERROR] 第 {successCount + 1} 筆執行失敗: {ex.Message}");
                return BadRequest(new { ok = false, error = $"執行失敗：{ex.Message}" });
            }
        }

        Console.WriteLine($"[INSERT] 完成匯入，成功: {successCount}/{qCnt} 筆");
        return Ok(new { ok = true, rows = successCount });
    }

    // --- DTO ---
    public class FetchReq
    {
        public string? PONum { get; set; }           // 訂單號
        public int? SerialNum { get; set; }          // 項次
        public string? PartNum { get; set; }         // 料號
        public string? Revision { get; set; }        // 版序
        public string? CustomerId { get; set; }      // 客戶代號
        public string? MatName { get; set; }         // 品名（保留）
        public string? MatClass { get; set; }        // 分類（保留）
        public string? PaperNum { get; set; }        // 製令單號（重要！）
        public bool IncludeMat { get; set; } = true; // 含材料（保留）
        public bool Calc { get; set; } = false;      // 計算可用（保留）
        public int? LessInq { get; set; }            // 缺料查詢：0=否, 1=是, 255=不限定
        public int? HasIssue { get; set; }           // 資料狀態：0=未發, 1=已發, 255=全部
        public int? POType { get; set; }             // 訂單類型：0=一般, 1=樣品, 2=工程, 255=全部

        // 任何多送的欄位都吃掉，不拋錯
        [JsonExtensionData] public Dictionary<string, object>? Extra { get; set; }
    }

    public class InsertReq
    {
        public string DllPaperNum { get; set; }   // 目的單號 = 製令單號
        public bool Replace { get; set; }         // 取代已存在
        public bool LessInq { get; set; }         // 缺料查詢
        public List<InsertRow> Rows { get; set; } = new();
    }

    public class InsertRow
    {
        public string PONum { get; set; }        // 訂單號（來源）
        public int SerialNum { get; set; }       // 項次（來源）
        public int POType { get; set; }          // 訂單類型
        public string MasPartNum { get; set; }   // 主料號
        public string MasRevision { get; set; }  // 主版序
        public decimal? StockQnty { get; set; }  // 庫存量
        public decimal? WIP { get; set; }        // 在製品
        public decimal? AllOutQnty { get; set; } // 總需求量
        public decimal? NeedQnty { get; set; }   // 缺料量
        public decimal? IssQnty { get; set; }    // 已發量
        public decimal? UnIssQnty { get; set; }  // 未發量
        public int POKind { get; set; }          // 訂單種類
    }
}
