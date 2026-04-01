using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
public class MPHdOutInvoiceController : ControllerBase
{
    private readonly string _cs;

    public MPHdOutInvoiceController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? db.Database.GetDbConnection().ConnectionString;
    }

    public class SaveRequest
    {
        public string paperNum { get; set; } = "";
        public string invoiceNum { get; set; } = "";
        public string invoiceType { get; set; } = "";
        public string invoiceDate { get; set; } = "";
        public string expectDate { get; set; } = "";
        public string userId { get; set; } = "";
    }

    [HttpGet("Init")]
    public async Task<IActionResult> Init(string paperNum)
    {
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "單號不可空白" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var payWays = new List<object>();
        await using (var cmd = new SqlCommand(
            "select PayWayCode, PayWayName from AJNdClassPayWay(nolock) order by PayWayCode", conn))
        await using (var rd = await cmd.ExecuteReaderAsync())
        {
            while (await rd.ReadAsync())
            {
                payWays.Add(new
                {
                    payWayCode = rd["PayWayCode"] == DBNull.Value ? 0 : Convert.ToInt32(rd["PayWayCode"]),
                    payWayName = rd["PayWayName"]?.ToString() ?? ""
                });
            }
        }

        var invoiceTypes = new List<object>();
        await using (var cmd = new SqlCommand(
            "select InvoiceType=InvoiceTypeId, InvoiceTypeName from AJNdClassInvoiceType(nolock) order by InvoiceTypeId", conn))
        await using (var rd = await cmd.ExecuteReaderAsync())
        {
            while (await rd.ReadAsync())
            {
                invoiceTypes.Add(new
                {
                    invoiceType = rd["InvoiceType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["InvoiceType"]),
                    invoiceTypeName = rd["InvoiceTypeName"]?.ToString() ?? ""
                });
            }
        }

        string customerId = "";
        int payWayCode = 0;
        string invoiceNum = "";
        int invoiceType = 0;
        DateTime? invoiceDate = null;
        DateTime? expectDate = null;

        await using (var cmd = new SqlCommand("exec MPHdDLLOutInvData @PaperNum", conn))
        {
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await using var rd = await cmd.ExecuteReaderAsync();
            if (await rd.ReadAsync())
            {
                customerId = rd["CustomerId"]?.ToString() ?? "";
                payWayCode = rd["PayWayCode"] == DBNull.Value ? 0 : Convert.ToInt32(rd["PayWayCode"]);
                invoiceNum = rd["InvoiceNum"]?.ToString() ?? "";
                invoiceType = rd["InvoiceType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["InvoiceType"]);
                invoiceDate = rd["InvoiceDate"] == DBNull.Value ? null : Convert.ToDateTime(rd["InvoiceDate"]);
                expectDate = rd["ExpectDate"] == DBNull.Value ? null : Convert.ToDateTime(rd["ExpectDate"]);
            }
        }

        var showToNtdHint = true;
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            await using var cmd = new SqlCommand(
                "select count(1) from AJNdCompanySystem with(nolock) where ToNTD=0 and CompanyId=@CompanyId", conn);
            cmd.Parameters.AddWithValue("@CompanyId", customerId);
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            showToNtdHint = count <= 0;
        }

        return Ok(new
        {
            ok = true,
            data = new
            {
                customerId,
                payWayCode,
                invoiceNum,
                invoiceType,
                invoiceDate,
                expectDate
            },
            payWays,
            invoiceTypes,
            showToNtdHint
        });
    }

    [HttpGet("ExpectDate")]
    public async Task<IActionResult> ExpectDate(string useId, string invoiceDate, string payWayCode, string customerId)
    {
        if (string.IsNullOrWhiteSpace(useId) ||
            string.IsNullOrWhiteSpace(invoiceDate) ||
            string.IsNullOrWhiteSpace(payWayCode) ||
            string.IsNullOrWhiteSpace(customerId))
        {
            return Ok(new { ok = false, error = "參數不足" });
        }

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("exec MPHdInvoiceExpectDate @UseId, @InvoiceDate, @PayWayCode, @CustomerId", conn);
        cmd.Parameters.AddWithValue("@UseId", useId);
        cmd.Parameters.AddWithValue("@InvoiceDate", invoiceDate);
        cmd.Parameters.AddWithValue("@PayWayCode", payWayCode);
        cmd.Parameters.AddWithValue("@CustomerId", customerId);
        await using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
            return Ok(new { ok = true, expectDate = rd["ExpectDate"]?.ToString() ?? "" });

        return Ok(new { ok = false, error = "查無付款日期" });
    }

    [HttpPost("Save")]
    public async Task<IActionResult> Save([FromBody] SaveRequest body)
    {
        if (string.IsNullOrWhiteSpace(body.paperNum))
            return Ok(new { ok = false, error = "單號不可空白" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        try
        {
            await using var cmd = new SqlCommand(
                "exec MPHdOutInvoice @PaperNum, @InvoiceNum, @InvoiceType, @InvoiceDate, @ExpectDate, @Flag, @UserId", conn);
            cmd.Parameters.AddWithValue("@PaperNum", body.paperNum ?? "");
            cmd.Parameters.AddWithValue("@InvoiceNum", body.invoiceNum ?? "");
            cmd.Parameters.AddWithValue("@InvoiceType", body.invoiceType ?? "");
            cmd.Parameters.AddWithValue("@InvoiceDate", body.invoiceDate ?? "");
            cmd.Parameters.AddWithValue("@ExpectDate", body.expectDate ?? "");
            cmd.Parameters.AddWithValue("@Flag", "1");
            cmd.Parameters.AddWithValue("@UserId", body.userId ?? "");
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            return Ok(new { ok = false, error = ex.Message });
        }

        return Ok(new { ok = true });
    }
}
