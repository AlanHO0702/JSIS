// Controllers/OrderDetailSearchController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json.Serialization;

[ApiController]
[Route("api/[controller]")]
public class OrderDetailSearchController : ControllerBase
{
    private readonly string _cs;
    public OrderDetailSearchController(IConfiguration cfg)
        => _cs = cfg.GetConnectionString("DefaultConnection");

    // 工具：null → DBNull
    static object Db(object v) => v is null ? DBNull.Value : v;

    // ① 查詢：SPodOCXOrderPOChice
// ① 查詢：SPodOCXOrderPOChice
[HttpPost("fetch")]
public async Task<IActionResult> Fetch([FromBody] FetchReq req)
{
    using var conn = new SqlConnection(_cs);
    using var cmd  = new SqlCommand("dbo.SPodOCXOrderPOChice", conn)
    { CommandType = CommandType.StoredProcedure };

    // 空→空字串
    string S(string? v) => (v ?? string.Empty).Trim();

    // 這三個一律送空字串而不是 DBNull
    cmd.Parameters.Add(new SqlParameter("@PartNum",  SqlDbType.NVarChar, 120){ Value = S(req.PartNum)  });
    cmd.Parameters.Add(new SqlParameter("@MatName",  SqlDbType.NVarChar, 200){ Value = S(req.MatName)  });
    cmd.Parameters.Add(new SqlParameter("@MatClass", SqlDbType.NVarChar,  20){ Value = S(req.MatClass) });

    // 固定參數
    cmd.Parameters.Add(new SqlParameter("@UseId",     SqlDbType.NVarChar, 20){ Value = "A001" });
    cmd.Parameters.Add(new SqlParameter("@QntyOver0", SqlDbType.Int){ Value = 255 });

    // 布林→int(1/0)
    cmd.Parameters.Add(new SqlParameter("@IncludeMat", SqlDbType.Int){ Value = req.IncludeMat ? 1 : 0 });
    cmd.Parameters.Add(new SqlParameter("@Calc",       SqlDbType.Int){ Value = req.Calc       ? 1 : 0 });

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
    return Ok(new { ok = true, data = rows });
}


    // ② 匯入：SPodOCXOrderDtAdd（逐筆）
    [HttpPost("insert")]
    public async Task<IActionResult> Insert([FromBody] InsertReq req)
    {
        if (string.IsNullOrWhiteSpace(req.DllPaperNum))
            return BadRequest("缺少目的單號 DllPaperNum。");
        if (req.Rows is null || req.Rows.Count == 0)
            return BadRequest("沒有要匯入的資料。");

        using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        int qCnt = req.Rows.Count;
        foreach (var r in req.Rows)
        {
            using var cmd = new SqlCommand("dbo.SPOdOCXOrderDtlAdd", conn) { CommandType = CommandType.StoredProcedure };
                // ✅ 改正：@PaperNum
                cmd.Parameters.Add(new SqlParameter("@PaperNum", SqlDbType.NVarChar, 16) 
                { Value = req.DllPaperNum });

                // ✅ 保持一樣：@PartNum
                cmd.Parameters.Add(new SqlParameter("@PartNum", SqlDbType.NVarChar, 120) 
                { Value = Db(r.PartNum) });

                // ✅ 保持一樣：@StockQnty
                cmd.Parameters.Add(new SqlParameter("@StockQnty", SqlDbType.Decimal) 
                { Precision = 18, Scale = 6, Value = r.StockQnty });

                // ✅ 保持一樣：@CanUseQnty
                cmd.Parameters.Add(new SqlParameter("@CanUseQnty", SqlDbType.Decimal) 
                { Precision = 18, Scale = 6, Value = r.CanUseQnty });

                // ✅ 改正：@Replace 是 int，不是 bit
                cmd.Parameters.Add(new SqlParameter("@Replace", SqlDbType.Int) 
                { Value = req.Replace ? 1 : 0 });

                // ✅ 改正：@Cnt
                cmd.Parameters.Add(new SqlParameter("@Cnt", SqlDbType.Int) 
                { Value = qCnt });

            await cmd.ExecuteNonQueryAsync();
        }

        return Ok(new { ok = true, rows = qCnt });
    }

    // --- DTO ---
    public class FetchReq
    {
        public string? PartNum { get; set; }
        public string? MatName { get; set; }
        public string? MatClass { get; set; }
        public bool IncludeMat { get; set; } = true;
        public bool Calc { get; set; } = false;

                // 任何多送的欄位都吃掉，不拋錯
        [JsonExtensionData] public Dictionary<string, object>? Extra { get; set; }
    }

    public class InsertReq
    {
        public string DllPaperNum { get; set; }   // 目的單號 = 當前單號
        public bool Replace { get; set; }         // 取代已存在
        public List<InsertRow> Rows { get; set; } = new();
    }
    public class InsertRow
    {
        public string PartNum { get; set; }
        public decimal StockQnty { get; set; }   // 你要傳入的庫存量
        public decimal CanUseQnty { get; set; }  // 可用量
    }
}
