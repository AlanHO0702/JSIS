using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
public class SPOdMPSOutInvoiceController : ControllerBase
{
    private readonly string _cs;

    public SPOdMPSOutInvoiceController(IConfiguration cfg, PcbErpContext db)
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
            return Ok(new { ok = false, error = "沒有資料" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        string cusId = "";
        await using (var cmd = new SqlCommand(
            "select Value from CURdSysParams(nolock) where SystemId='EMO' and ParamId='CusId'", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
                cusId = reader["Value"]?.ToString() ?? "";
        }

        var checkSql = @"
select count(1)
  from SPOdMPSOutMain(nolock)
 where PaperNum=@PaperNum and Finished in (1,4)";
        await using (var cmd = new SqlCommand(checkSql, conn))
        {
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            var cnt = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            if (cnt < 1)
                return Ok(new { ok = false, error = "單據未完成，不可開立發票!!" });
        }

        var invoiceTypes = new List<object>();
        await using (var cmd = new SqlCommand(
            "select InvoiceTypeId, InvoiceTypeName, TaxRate from AJNdClassInvoiceType(nolock)", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                invoiceTypes.Add(new
                {
                    InvoiceTypeId = reader["InvoiceTypeId"]?.ToString(),
                    InvoiceTypeName = reader["InvoiceTypeName"]?.ToString(),
                    TaxRate = reader["TaxRate"]?.ToString()
                });
            }
        }

        string invSql = $@"
select InvoiceNum, InvoiceType,
       InvoiceDate={(cusId == "SS" ? "PaperDate" : "InvoiceDate")},
       ExpectDate, CustomerId, PayWayCode
  from SPOdMPSOutMain(nolock)
 where PaperNum=@PaperNum";
        string invoiceNum = "";
        string invoiceType = "";
        DateTime? invoiceDate = null;
        DateTime? expectDate = null;
        string customerId = "";
        string payWayCode = "";
        await using (var cmd = new SqlCommand(invSql, conn))
        {
            cmd.Parameters.AddWithValue("@PaperNum", paperNum);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                invoiceNum = reader["InvoiceNum"]?.ToString() ?? "";
                invoiceType = reader["InvoiceType"]?.ToString() ?? "";
                invoiceDate = reader["InvoiceDate"] as DateTime?;
                expectDate = reader["ExpectDate"] as DateTime?;
                customerId = reader["CustomerId"]?.ToString() ?? "";
                payWayCode = reader["PayWayCode"]?.ToString() ?? "";
            }
        }

        bool selectInvWord = await HasSysParam(conn, "ATX", "SelectInvWord");
        bool useSameInvWord = await HasSysParam(conn, "ATX", "UseSameInvWork");
        bool lockExpectDate = await HasSysParam(conn, "SPO", "OutInvLockExpDate");
        bool outInvAct = await HasSysParam(conn, "SPO", "OutInvAct");

        var invWordList = Array.Empty<object>();
        if (selectInvWord)
        {
            var invDateText = invoiceDate?.ToString("yyyy/MM/dd") ?? "";
            invWordList = await LoadInvWordList(conn, invDateText, useSameInvWord);
        }

        return Ok(new
        {
            ok = true,
            data = new
            {
                invoiceNum,
                invoiceType,
                invoiceDate,
                expectDate,
                customerId,
                payWayCode
            },
            invoiceTypes,
            selectInvWord,
            useSameInvWord,
            invWords = invWordList,
            lockExpectDate,
            outInvAct
        });
    }

    [HttpGet("InvWords")]
    public async Task<IActionResult> InvWords(string invoiceDate, int useSame = 0)
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        var list = await LoadInvWordList(conn, invoiceDate, useSame == 1);
        return Ok(new { ok = true, list });
    }

    [HttpGet("GetNum")]
    public async Task<IActionResult> GetNum(string invoiceDate, string invWord, string paperNum, string invoiceType)
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var sql = "exec SPOdInDLLGetInvoiceNum @InvoiceDate, @InvWord, @PaperNum, @InvoiceType";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@InvoiceDate", invoiceDate ?? "");
        cmd.Parameters.AddWithValue("@InvWord", invWord ?? "");
        cmd.Parameters.AddWithValue("@PaperNum", paperNum ?? "");
        cmd.Parameters.AddWithValue("@InvoiceType", invoiceType ?? "");
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return Ok(new { ok = true, invoiceNum = reader["InvoiceNum"]?.ToString() ?? "" });

        return Ok(new { ok = false, error = "取得發票號碼失敗" });
    }

    [HttpGet("ExpectDate")]
    public async Task<IActionResult> ExpectDate(string useId, string invoiceDate, string payWayCode, string customerId)
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var sql = "exec SPOdInvoiceExpectDate @UseId, @InvoiceDate, @PayWayCode, @CustomerId";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UseId", useId ?? "");
        cmd.Parameters.AddWithValue("@InvoiceDate", invoiceDate ?? "");
        cmd.Parameters.AddWithValue("@PayWayCode", payWayCode ?? "");
        cmd.Parameters.AddWithValue("@CustomerId", customerId ?? "");
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return Ok(new { ok = true, expectDate = reader["ExpectDate"]?.ToString() ?? "" });

        return Ok(new { ok = false, error = "取得付款日期失敗" });
    }

    [HttpPost("Save")]
    public async Task<IActionResult> Save([FromBody] SaveRequest body)
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var sql = "exec SPOdMPSOutInvoice @PaperNum, @InvoiceNum, @InvoiceType, @InvoiceDate, @ExpectDate, @Flag, @UserId";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PaperNum", body.paperNum ?? "");
        cmd.Parameters.AddWithValue("@InvoiceNum", body.invoiceNum ?? "");
        cmd.Parameters.AddWithValue("@InvoiceType", body.invoiceType ?? "");
        cmd.Parameters.AddWithValue("@InvoiceDate", body.invoiceDate ?? "");
        cmd.Parameters.AddWithValue("@ExpectDate", body.expectDate ?? "");
        cmd.Parameters.AddWithValue("@Flag", "1");
        cmd.Parameters.AddWithValue("@UserId", body.userId ?? "");
        await cmd.ExecuteNonQueryAsync();

        bool outInvAct = await HasSysParam(conn, "SPO", "OutInvAct");
        return Ok(new { ok = true, outInvAct });
    }

    private static async Task<bool> HasSysParam(SqlConnection conn, string systemId, string paramId)
    {
        var sql = "select Value from CURdSysParams(nolock) where SystemId=@sys and ParamId=@pid and Value='1'";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@sys", systemId);
        cmd.Parameters.AddWithValue("@pid", paramId);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }

    private static async Task<object[]> LoadInvWordList(SqlConnection conn, string invoiceDate, bool useSame)
    {
        var list = new List<object>();
        var sql = useSame
            ? "select Item=InvWord, ItemName=convert(varchar,TaxTypeId) from ATXdVInvWordSel_Plus(nolock) where @InvoiceDate between BDate and EDate"
            : "select Item=InvWord, ItemName='格式 '+convert(varchar,TaxTypeId) from ATXdVInvWordSel(nolock) where @InvoiceDate between BDate and EDate";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@InvoiceDate", invoiceDate ?? "");
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new
            {
                Item = reader["Item"]?.ToString() ?? "",
                ItemName = reader["ItemName"]?.ToString() ?? ""
            });
        }
        return list.ToArray();
    }
}
