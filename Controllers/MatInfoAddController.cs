using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MatInfoAddController : ControllerBase
{
    private readonly string _connStr;

    public MatInfoAddController(PcbErpContext context, IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")
            ?? context?.Database.GetDbConnection().ConnectionString
            ?? throw new InvalidOperationException("Missing connection string.");
    }

    public class GenNumRequest
    {
        public int Spid { get; set; }
        public string SetClass { get; set; } = "";
        public int Status { get; set; }
    }

    public class ImportPropRequest
    {
        public string SetClass { get; set; } = "";
        public string ReferencePN { get; set; } = "";
        public int Spid { get; set; }
    }

    public class CheckAddRequest
    {
        public string PartNum { get; set; } = "";
        public string MatClass { get; set; } = "";
    }

    public class AddRequest
    {
        public string PartNum { get; set; } = "";
        public string SetClass { get; set; } = "";
        public string MatName { get; set; } = "";
        public int MB { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; } = "";
        public string ReferencePN { get; set; } = "";
        public int Spid { get; set; }
        public string Unit { get; set; } = "";
        public int IsTmpPN2Real { get; set; }
        public string MergePartNum { get; set; } = "";
        public string CurrOldPN { get; set; } = "";
        public string GlobalId4Add { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string PartNumAddForJH { get; set; } = "";
        public int JH_B { get; set; }
        public int JH_C { get; set; }
        public string MultiBuToJH_B { get; set; } = "";
        public string MultiBuToJH_C { get; set; } = "";
    }

    public class DoCallRequest
    {
        public string GlobalId { get; set; } = "";
        public int Spid { get; set; }
        public int PowerType { get; set; }
    }

    public class UpdateAddDataRequest
    {
        public int Spid { get; set; }
        public string SetClass { get; set; } = "";
        public string NumId { get; set; } = "";
        public string DtlNumId { get; set; } = "";
    }

    private static void AddParam(SqlCommand cmd, string name, object? value)
    {
        cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    private static async Task<bool> HasProcParamAsync(SqlConnection conn, string procName, string paramName)
    {
        const string sql = @"
SELECT 1
  FROM sys.parameters p
  JOIN sys.objects o ON o.object_id = p.object_id
 WHERE o.type = 'P'
   AND o.name = @proc
   AND p.name = @param";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@proc", procName);
        cmd.Parameters.AddWithValue("@param", paramName);
        var result = await cmd.ExecuteScalarAsync();
        return result != null && result != DBNull.Value;
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
    public async Task<IActionResult> GetMatClass(int isForEMO = 1, int mb = 0)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdMatClass4Add", conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        AddParam(cmd, "@IsForEMO", isForEMO);
        AddParam(cmd, "@MB", mb);
        var rows = await ReadListAsync(cmd);
        return Ok(rows);
    }

    [HttpPost("GenNum")]
    public async Task<IActionResult> GenNum([FromBody] GenNumRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.SetClass))
            return BadRequest(new { ok = false, error = "SetClass required." });
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdGenSetNumAdd", conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        AddParam(cmd, "@Spid", req.Spid);
        AddParam(cmd, "@SetClass", req.SetClass);
        AddParam(cmd, "@Status", req.Status);
        var rows = await ReadListAsync(cmd);
        return Ok(new { ok = true, rows });
    }

    [HttpGet("AddData")]
    public async Task<IActionResult> GetAddData(string setClass, string? currOldPN, int? spid)
    {
        if (string.IsNullOrWhiteSpace(setClass))
            return BadRequest(new { ok = false, error = "SetClass required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        int spidValue = spid ?? 0;
        if (spidValue <= 0)
        {
            await using var genCmd = new SqlCommand("MGNdGenSetNumTable", conn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };
            AddParam(genCmd, "@SetClass", setClass);
            AddParam(genCmd, "@PartNum", currOldPN ?? "");
            AddParam(genCmd, "@Kind", 0);
            await using var reader = await genCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                spidValue = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
        }

        if (spidValue <= 0)
            return StatusCode(500, new { ok = false, error = "Spid not created." });

        string? enCode = null;
        await using (var encodeCmd = new SqlCommand("select EnCode from MGNdSetNumMain(nolock) where SetClass=@SetClass", conn))
        {
            AddParam(encodeCmd, "@SetClass", setClass);
            enCode = (await encodeCmd.ExecuteScalarAsync())?.ToString();
        }

        await using var cmd = new SqlCommand(@"
select * from MGNdSetNumAddData(nolock)
where spid = @Spid and SetClass = @SetClass
order by iSeq", conn);
        AddParam(cmd, "@Spid", spidValue);
        AddParam(cmd, "@SetClass", setClass);
        var rows = await ReadListAsync(cmd);
        return Ok(new { ok = true, spid = spidValue, enCode, rows });
    }

    [HttpGet("ReferencePN")]
    public async Task<IActionResult> GetReferencePn(string? matClass)
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdReferencePN", conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        AddParam(cmd, "@MatClass", matClass ?? "");
        var rows = await ReadListAsync(cmd);
        return Ok(rows);
    }

    [HttpGet("Units")]
    public async Task<IActionResult> GetUnits()
    {
        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("select Unit, Notes from MINdUnitBasic(nolock)", conn);
        var rows = await ReadListAsync(cmd);
        return Ok(rows);
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

    [HttpPost("ImportProp")]
    public async Task<IActionResult> ImportProp([FromBody] ImportPropRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.SetClass) || string.IsNullOrWhiteSpace(req.ReferencePN))
            return BadRequest(new { ok = false, error = "SetClass and ReferencePN required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdImportPropFromRefer", conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        AddParam(cmd, "@SetClass", req.SetClass);
        AddParam(cmd, "@ReferencePN", req.ReferencePN);
        AddParam(cmd, "@spid", req.Spid);
        await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = true });
    }

    [HttpPost("UpdateAddData")]
    public async Task<IActionResult> UpdateAddData([FromBody] UpdateAddDataRequest req)
    {
        if (req == null || req.Spid <= 0 || string.IsNullOrWhiteSpace(req.SetClass)
            || string.IsNullOrWhiteSpace(req.NumId) || string.IsNullOrWhiteSpace(req.DtlNumId))
            return BadRequest(new { ok = false, error = "Spid, SetClass, NumId, DtlNumId required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();

        string? dtlNumName = null;
        string? enCode = null;
        await using (var lookup = new SqlCommand(@"
select DtlNumName, EnCode
from MGNdSetNumSubDtl(nolock)
where SetClass = @SetClass and NumId = @NumId and DtlNumId = @DtlNumId", conn))
        {
            AddParam(lookup, "@SetClass", req.SetClass);
            AddParam(lookup, "@NumId", req.NumId);
            AddParam(lookup, "@DtlNumId", req.DtlNumId);
            await using var reader = await lookup.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dtlNumName = reader.IsDBNull(0) ? null : reader.GetString(0);
                enCode = reader.IsDBNull(1) ? null : reader.GetString(1);
            }
        }

        if (dtlNumName == null)
            return NotFound(new { ok = false, error = "DtlNumId not found." });

        await using (var update = new SqlCommand(@"
update MGNdSetNumAddData
set DtlNumId = @DtlNumId, DtlNumName = @DtlNumName, EnCode = @EnCode
where Spid = @Spid and SetClass = @SetClass and NumId = @NumId", conn))
        {
            AddParam(update, "@DtlNumId", req.DtlNumId);
            AddParam(update, "@DtlNumName", dtlNumName);
            AddParam(update, "@EnCode", enCode ?? "");
            AddParam(update, "@Spid", req.Spid);
            AddParam(update, "@SetClass", req.SetClass);
            AddParam(update, "@NumId", req.NumId);
            await update.ExecuteNonQueryAsync();
        }

        return Ok(new { ok = true, DtlNumName = dtlNumName, EnCode = enCode ?? "" });
    }

    [HttpPost("CheckAdd")]
    public async Task<IActionResult> CheckAdd([FromBody] CheckAddRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PartNum) || string.IsNullOrWhiteSpace(req.MatClass))
            return BadRequest(new { ok = false, error = "PartNum and MatClass required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdMatInfoChkAdd", conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        AddParam(cmd, "@PartNum", req.PartNum);
        AddParam(cmd, "@MatClass", req.MatClass);
        var rows = await ReadListAsync(cmd);
        var result = rows.Count > 0 && rows[0].TryGetValue("Result", out var v) ? v : 0;
        return Ok(new { Result = result ?? 0, rows });
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] AddRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.PartNum) || string.IsNullOrWhiteSpace(req.SetClass))
            return BadRequest(new { ok = false, error = "PartNum and SetClass required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdPartNumAdd", conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        AddParam(cmd, "@PartNum", req.PartNum);
        AddParam(cmd, "@MatClass", req.SetClass);
        AddParam(cmd, "@MatName", req.MatName);
        AddParam(cmd, "@MB", req.MB);
        AddParam(cmd, "@Status", req.Status);
        AddParam(cmd, "@Build_UserId", req.UserId);
        AddParam(cmd, "@Ref_PartNum", req.ReferencePN);
        AddParam(cmd, "@SPID", req.Spid);
        AddParam(cmd, "@Unit", req.Unit);
        AddParam(cmd, "@IsTmpPN2Real", req.IsTmpPN2Real);
        AddParam(cmd, "@MergePartNum", req.MergePartNum);
        AddParam(cmd, "@OldPN", req.CurrOldPN);
        AddParam(cmd, "@GlobalId4Add", req.GlobalId4Add);
        AddParam(cmd, "@sAccountType", req.AccountType);
        AddParam(cmd, "@sPartNumAddForJH", req.PartNumAddForJH);
        AddParam(cmd, "@chbx_JH_B", req.JH_B);
        AddParam(cmd, "@chbx_JH_C", req.JH_C);
        AddParam(cmd, "@sMultiBuToJH_B", req.MultiBuToJH_B);
        AddParam(cmd, "@sMultiBuToJH_C", req.MultiBuToJH_C);
        AddParam(cmd, "@EngGauge", "");
        await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = true });
    }

    [HttpPost("DoCall")]
    public async Task<IActionResult> DoCall([FromBody] DoCallRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.GlobalId) || req.Spid <= 0)
            return BadRequest(new { ok = false, error = "GlobalId and Spid required." });

        await using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("MGNdMatInfoDoCALL", conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        AddParam(cmd, "@GlobalId", req.GlobalId);
        AddParam(cmd, "@Spid", req.Spid);
        AddParam(cmd, "@PowerType", req.PowerType);
        await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = true });
    }
}
