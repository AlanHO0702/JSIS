using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;

/// <summary>
/// ATX00002 - 營業稅單據批次輸入 自訂按鈕 API 控制器
/// 對應 Delphi: BatchDefaultDLL.pas
/// 功能:
///   - btnC1: 預設資料 — 編輯 ATXdCertifBatchDefault 資料表
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ATX00002Controller : ControllerBase
{
    private readonly string _cs;

    public ATX00002Controller(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到連線字串");
    }

    /// <summary>
    /// 取得 ATXdCertifBatchDefault 全部資料
    /// GET /api/ATX00002/GetBatchDefault
    /// </summary>
    [HttpGet("GetBatchDefault")]
    public async Task<IActionResult> GetBatchDefault()
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        // 主資料
        var rows = new List<Dictionary<string, object?>>();
        const string sql = "SELECT * FROM ATXdCertifBatchDefault WITH (NOLOCK)";
        await using var cmd = new SqlCommand(sql, conn);
        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < rdr.FieldCount; i++)
                row[rdr.GetName(i)] = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
            rows.Add(row);
        }
        await rdr.CloseAsync();

        // Lookup 資料 (對應 Delphi .dfm 的各 qry 定義)
        var lookups = new Dictionary<string, object>();
        var lookupDefs = new[]
        {
            new { Name = "InvoiceTypeId", Sql = "SELECT InvoiceTypeId AS [key], InvoiceTypeName AS [display] FROM AJNdClassInvoiceType WITH (NOLOCK) ORDER BY InvoiceTypeId" },
            new { Name = "TaxTypeId",     Sql = "SELECT TaxTypeId AS [key], TaxTypeName AS [display] FROM ATXdTaxType WITH (NOLOCK) ORDER BY TaxTypeId" },
            new { Name = "PayWayCode",    Sql = "SELECT PayWayCode AS [key], PayWayName AS [display] FROM AJNdClassPayWay WITH (NOLOCK) ORDER BY PayWayCode" },
            new { Name = "CertifTypeId",  Sql = "SELECT CertifTypeId AS [key], CertifTypeName AS [display] FROM APRdCertifType WITH (NOLOCK) ORDER BY CertifTypeId" },
            new { Name = "TaxCutTypeId",  Sql = "SELECT TaxCutTypeId AS [key], TaxCutTypeName AS [display] FROM ATXdTaxCutType WITH (NOLOCK) ORDER BY TaxCutTypeId" },
            new { Name = "MoneyCode",     Sql = "SELECT MoneyCode AS [key], MoneyName AS [display] FROM AJNdClassMoney WITH (NOLOCK) ORDER BY MoneyCode" },
        };

        foreach (var def in lookupDefs)
        {
            var list = new List<Dictionary<string, object?>>();
            try
            {
                await using var cmdLk = new SqlCommand(def.Sql, conn);
                await using var rdrLk = await cmdLk.ExecuteReaderAsync();
                while (await rdrLk.ReadAsync())
                {
                    list.Add(new Dictionary<string, object?>
                    {
                        ["key"] = rdrLk.IsDBNull(0) ? null : rdrLk.GetValue(0),
                        ["display"] = rdrLk.IsDBNull(1) ? null : rdrLk.GetValue(1)
                    });
                }
            }
            catch { /* 查表失敗不影響主流程 */ }
            lookups[def.Name] = list;
        }

        return Ok(new { ok = true, rows, lookups });
    }

    /// <summary>
    /// 依據 InvoiceTypeId + IsIn 取得對應 TaxTypeId，以及依據 InvoiceTypeId 取得 TaxCutTypeId
    /// GET /api/ATX00002/GetCascadeValues?invoiceTypeId=1&amp;isIn=1
    /// </summary>
    [HttpGet("GetCascadeValues")]
    public async Task<IActionResult> GetCascadeValues([FromQuery] int invoiceTypeId, [FromQuery] int isIn)
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        // 1. ATXdTaxTypeId SP → TaxTypeId
        int? taxTypeId = null;
        await using var cmdTax = new SqlCommand("ATXdTaxTypeId", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmdTax.Parameters.AddWithValue("@InvoiceTypeId", invoiceTypeId);
        cmdTax.Parameters.AddWithValue("@IsIn", isIn);

        await using var rdrTax = await cmdTax.ExecuteReaderAsync();
        if (await rdrTax.ReadAsync())
        {
            var ordinal = rdrTax.GetOrdinal("TaxTypeId");
            if (!rdrTax.IsDBNull(ordinal))
                taxTypeId = rdrTax.GetInt32(ordinal);
        }
        await rdrTax.CloseAsync();

        // 2. AJNdClassInvoiceType → TaxCutTypeId
        int? taxCutTypeId = null;
        const string sqlCut = "SELECT TaxCutTypeId FROM AJNdClassInvoiceType WITH (NOLOCK) WHERE InvoiceTypeId = @InvoiceTypeId";
        await using var cmdCut = new SqlCommand(sqlCut, conn);
        cmdCut.Parameters.AddWithValue("@InvoiceTypeId", invoiceTypeId);
        var result = await cmdCut.ExecuteScalarAsync();
        if (result != null && result != DBNull.Value)
            taxCutTypeId = Convert.ToInt32(result);

        return Ok(new { ok = true, taxTypeId, taxCutTypeId });
    }

    /// <summary>
    /// 儲存 ATXdCertifBatchDefault (固定一筆資料，直接全欄位 UPDATE)
    /// POST /api/ATX00002/SaveBatchDefault
    /// 對應 Delphi: btnOKClick → Post
    /// </summary>
    [HttpPost("SaveBatchDefault")]
    public async Task<IActionResult> SaveBatchDefault([FromBody] BatchDefaultRow row)
    {
        if (row == null)
            return Ok(new { ok = false, error = "無資料" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        // 依據 InvoiceTypeId + IsIn 取得正確的 TaxTypeId (對應 Delphi Validate 自動設定)
        await using var cmdTax = new SqlCommand("ATXdTaxTypeId", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmdTax.Parameters.AddWithValue("@InvoiceTypeId", row.InvoiceTypeId);
        cmdTax.Parameters.AddWithValue("@IsIn", row.IsIn);

        await using var rdr = await cmdTax.ExecuteReaderAsync();
        if (await rdr.ReadAsync())
        {
            var ordinal = rdr.GetOrdinal("TaxTypeId");
            if (!rdr.IsDBNull(ordinal))
                row.TaxTypeId = rdr.GetInt32(ordinal);
        }
        await rdr.CloseAsync();

        // 依據 InvoiceTypeId 取得正確的 TaxCutTypeId
        const string sqlCut = "SELECT TaxCutTypeId FROM AJNdClassInvoiceType WITH (NOLOCK) WHERE InvoiceTypeId = @InvId";
        await using var cmdCut = new SqlCommand(sqlCut, conn);
        cmdCut.Parameters.AddWithValue("@InvId", row.InvoiceTypeId);
        var cutResult = await cmdCut.ExecuteScalarAsync();
        if (cutResult != null && cutResult != DBNull.Value)
            row.TaxCutTypeId = Convert.ToInt32(cutResult);

        // 只有一筆 → 直接 UPDATE 全部欄位 (無 WHERE 條件)
        const string sql = @"
            UPDATE ATXdCertifBatchDefault SET
                IsIn            = @IsIn,
                IsInvoice       = @IsInvoice,
                InvoiceTypeId   = @InvoiceTypeId,
                TaxTypeId       = @TaxTypeId,
                PayWayCode      = @PayWayCode,
                CertifTypeId    = @CertifTypeId,
                TaxCutTypeId    = @TaxCutTypeId,
                MoneyCode       = @MoneyCode";

        await using var cmdUpd = new SqlCommand(sql, conn);
        cmdUpd.Parameters.AddWithValue("@IsIn", row.IsIn);
        cmdUpd.Parameters.AddWithValue("@IsInvoice", row.IsInvoice);
        cmdUpd.Parameters.AddWithValue("@InvoiceTypeId", row.InvoiceTypeId);
        cmdUpd.Parameters.AddWithValue("@TaxTypeId", row.TaxTypeId);
        cmdUpd.Parameters.AddWithValue("@PayWayCode", row.PayWayCode);
        cmdUpd.Parameters.AddWithValue("@CertifTypeId", row.CertifTypeId);
        cmdUpd.Parameters.AddWithValue("@TaxCutTypeId", row.TaxCutTypeId);
        cmdUpd.Parameters.AddWithValue("@MoneyCode", row.MoneyCode);

        await cmdUpd.ExecuteNonQueryAsync();
        return Ok(new { ok = true });
    }

    public class BatchDefaultRow
    {
        public int IsIn { get; set; }
        public int IsInvoice { get; set; }
        public int InvoiceTypeId { get; set; }
        public int TaxTypeId { get; set; }
        public int PayWayCode { get; set; }
        public int CertifTypeId { get; set; }
        public int TaxCutTypeId { get; set; }
        public int MoneyCode { get; set; }
    }
}