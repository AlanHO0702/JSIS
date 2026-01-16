using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MatInfoSearchController : ControllerBase
{
    private readonly string _connStr;

    public MatInfoSearchController(PcbErpContext context, IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? context?.Database.GetDbConnection().ConnectionString
            ?? throw new InvalidOperationException("Missing connection string.");
    }

    public class SearchAddDataRow
    {
        public string? NumId { get; set; }
        public string? DtlNumId_B { get; set; }
        public string? DtlNumId_E { get; set; }
    }

    public class MatInfoSearchRequest
    {
        public string? PartNumLike { get; set; }
        public string? PartNumB { get; set; }
        public string? PartNumE { get; set; }
        public string? MatName { get; set; }
        public string? MapNo { get; set; }
        public string? OldPartNum { get; set; }
        public string? EngGauge { get; set; }
        public string? PaperNum { get; set; }
        public string? CustomerPN { get; set; }
        public string? Barcode { get; set; }
        public string? FromTmpPartNum { get; set; }
        public string? NewPartNum { get; set; }
        public string? SetClass { get; set; }
        public string? MatClass1 { get; set; }
        public string? MatClass2 { get; set; }
        public string? UserId { get; set; }
        public int? Status { get; set; }
        public int? IsTrans { get; set; }
        public int? IsEMO { get; set; }
        public int? MB { get; set; }
        public int? iPNStatus { get; set; }
        public int? TranFacType { get; set; }
        public int? IsTranToC { get; set; }
        public int? IsTranFrom { get; set; }
        public DateTime? BuildDateStart { get; set; }
        public DateTime? BuildDateEnd { get; set; }
        public int? Limit { get; set; }
        public List<SearchAddDataRow>? AddData { get; set; }
    }

    private static void AddParam(SqlCommand cmd, string name, object? value)
    {
        cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    private static async Task<List<Dictionary<string, object?>>> ReadListAsync(SqlCommand cmd)
    {
        var results = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                row[name] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            results.Add(row);
        }
        while (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync()) { }
        }
        return results;
    }

    [HttpGet("MatClass")]
    public async Task<IActionResult> GetMatClass(int? mb = null)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        var sql = "Select MatClass, ClassName, MB = IsNull(MB, 0) From MINdMatClass (nolock)";
        if (mb is 0 or 1)
            sql += " where IsNull(MB, 0)=@MB";
        sql += " Order By MatClass";
        await using var cmd = new SqlCommand(sql, conn);
        if (mb is 0 or 1)
            AddParam(cmd, "@MB", mb);
        var rows = await ReadListAsync(cmd);
        return Ok(rows);
    }

    [HttpGet("Users")]
    public async Task<IActionResult> GetUsers()
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(@"
Select UserId, UserName
From CURdUsers (nolock)
Order By UserId", conn);
        var rows = await ReadListAsync(cmd);
        return Ok(rows);
    }

    [HttpGet("SetClassData")]
    public async Task<IActionResult> GetSetClassData(string setClass)
    {
        if (string.IsNullOrWhiteSpace(setClass))
            return BadRequest(new { ok = false, error = "SetClass required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        int spidValue = 0;
        await using (var genCmd = new SqlCommand("MGNdGenSetNumTable", conn))
        {
            genCmd.CommandType = CommandType.StoredProcedure;
            genCmd.CommandTimeout = 120;
            AddParam(genCmd, "@SetClass", setClass);
            AddParam(genCmd, "@PartNum", "");
            AddParam(genCmd, "@Kind", 1);
            await using var reader = await genCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                spidValue = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
        }

        if (spidValue <= 0)
            return StatusCode(500, new { ok = false, error = "Spid not created." });

        await using var cmd = new SqlCommand(@"
select * from MGNdSetNumAddData(nolock)
where spid = @Spid and SetClass = @SetClass
order by iSeq", conn);
        AddParam(cmd, "@Spid", spidValue);
        AddParam(cmd, "@SetClass", setClass);
        var rows = await ReadListAsync(cmd);
        return Ok(new { ok = true, spid = spidValue, rows });
    }

    [HttpGet("DtlNumOptions")]
    public async Task<IActionResult> GetDtlNumOptions(string setClass, string numId)
    {
        if (string.IsNullOrWhiteSpace(setClass) || string.IsNullOrWhiteSpace(numId))
            return BadRequest(new { ok = false, error = "SetClass and NumId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(@"
select DtlNumId, DtlNumName, EnCode
from MGNdSetNumSubDtl(nolock)
where SetClass = @SetClass and NumId = @NumId", conn);
        AddParam(cmd, "@SetClass", setClass);
        AddParam(cmd, "@NumId", numId);
        var rows = await ReadListAsync(cmd);
        return Ok(rows);
    }

    [HttpPost("Search")]
    public async Task<IActionResult> Search([FromBody] MatInfoSearchRequest req)
    {
        var limit = req.Limit.HasValue ? Math.Clamp(req.Limit.Value, 1, 500) : 200;

        var where = new List<string>();
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandTimeout = 180;

        cmd.CommandText = "select top (@Limit) * from MINdMatInfo (nolock) where 1=1";
        AddParam((SqlCommand)cmd, "@Limit", limit);

        void AddLike(string column, string? value, string param)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            where.Add($"and {column} like @{param}");
            AddParam((SqlCommand)cmd, $"@{param}", $"%{value.Trim()}%");
        }

        void AddEq<T>(string column, T? value, string param) where T : struct
        {
            if (!value.HasValue) return;
            where.Add($"and {column} = @{param}");
            AddParam((SqlCommand)cmd, $"@{param}", value.Value);
        }

        void AddEqStr(string column, string? value, string param)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            where.Add($"and {column} = @{param}");
            AddParam((SqlCommand)cmd, $"@{param}", value.Trim());
        }

        AddLike("PartNum", req.PartNumLike, "PartNumLike");

        if (!string.IsNullOrWhiteSpace(req.PartNumB))
        {
            where.Add("and PartNum >= @PartNumBRange");
            AddParam((SqlCommand)cmd, "@PartNumBRange", req.PartNumB.Trim());
        }
        if (!string.IsNullOrWhiteSpace(req.PartNumE))
        {
            where.Add("and PartNum <= @PartNumERange");
            AddParam((SqlCommand)cmd, "@PartNumERange", req.PartNumE.Trim());
        }

        AddLike("MatName", req.MatName, "MatName");
        AddLike("MapNo", req.MapNo, "MapNo");
        AddLike("OldPartNum", req.OldPartNum, "OldPartNum");
        AddLike("EngGauge", req.EngGauge, "EngGauge");
        AddLike("PaperNum", req.PaperNum, "PaperNum");
        AddLike("CustomerPartNum", req.CustomerPN, "CustomerPN");
        AddLike("BarCode", req.Barcode, "BarCode");
        AddLike("FromTmpPartNum", req.FromTmpPartNum, "FromTmpPartNum");
        AddLike("NewPartNum", req.NewPartNum, "NewPartNum");

        AddEqStr("MatClass", req.SetClass, "SetClass");
        AddEqStr("MatClass1", req.MatClass1, "MatClass1");
        AddEqStr("MatClass2", req.MatClass2, "MatClass2");
        AddEqStr("Build_UserId", req.UserId, "BuildUserId");
        AddEq("Status", req.Status, "Status");
        AddEq("IsTrans", req.IsTrans, "IsTrans");
        AddEq("IsEMO", req.IsEMO, "IsEMO");
        AddEq("MB", req.MB, "MB");
        AddEq("iPNStatus", req.iPNStatus, "iPNStatus");
        AddEq("TranFacType", req.TranFacType, "TranFacType");
        AddEq("IsTranToC", req.IsTranToC, "IsTranToC");
        AddEq("IsTranFrom", req.IsTranFrom, "IsTranFrom");

        if (req.BuildDateStart.HasValue)
        {
            where.Add("and Build_Date >= @BuildDateStart");
            AddParam((SqlCommand)cmd, "@BuildDateStart", req.BuildDateStart.Value);
        }
        if (req.BuildDateEnd.HasValue)
        {
            where.Add("and Build_Date <= @BuildDateEnd");
            AddParam((SqlCommand)cmd, "@BuildDateEnd", req.BuildDateEnd.Value);
        }

        cmd.CommandText += " " + string.Join(" ", where) + " order by PartNum";
        var rows = await ReadListAsync((SqlCommand)cmd);
        return Ok(new { ok = true, data = rows });
    }
}
