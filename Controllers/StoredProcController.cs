using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StoredProcController : ControllerBase
{
    private readonly string _cs;

    // 白名單：前端 key -> SP 名稱 + 必填/可選參數
    private static readonly Dictionary<string, StoredProcDef> _registry =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["CalcOrderAmount"] = new StoredProcDef(
            ProcName: "SPOdOrderTotal",
            RequiredParams: new[] { "PaperNum" }
        )
    };

    public StoredProcController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("Default")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("找不到 ConnectionStrings:Default 或 DbContext 連線字串");
    }

    public record ExecSpRequest(string Key, Dictionary<string, object>? Args);
    private record StoredProcDef(string ProcName, string[] RequiredParams, string[]? OptionalParams = null);

    [HttpPost("exec")]
    public async Task<IActionResult> Exec([FromBody] ExecSpRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.Key) || !_registry.TryGetValue(req.Key, out var def))
            return BadRequest(new { ok = false, error = "未知的作業代碼" });

        var args = req.Args ?? new();
        foreach (var p in def.RequiredParams)
            if (!args.ContainsKey(p))
                return BadRequest(new { ok = false, error = $"缺少參數: {p}" });

        await using var conn = new SqlConnection(_cs);
        SqlTransaction? tx = null;

        try
        {
            await conn.OpenAsync();
            tx = (SqlTransaction)await conn.BeginTransactionAsync();

            await using var cmd = new SqlCommand(def.ProcName, conn, tx)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            // 👇 這個 helper 把 JsonElement 轉成 .NET 基本型別
            static object? ToClr(object? v)
            {
                if (v is null) return DBNull.Value;
                if (v is not JsonElement je) return v;

                return je.ValueKind switch
                {
                    JsonValueKind.Null or JsonValueKind.Undefined => DBNull.Value,
                    JsonValueKind.String   => je.GetString(),
                    JsonValueKind.Number   => je.TryGetInt64(out var l) ? l :
                                            je.TryGetDecimal(out var d) ? d :
                                            je.GetDouble(),
                    JsonValueKind.True     => true,
                    JsonValueKind.False    => false,
                    // 參數不會用到 Object/Array；保險：回原字串
                    _ => je.GetRawText()
                };
            }

            // 白名單必填參數
            foreach (var p in def.RequiredParams)
                cmd.Parameters.AddWithValue("@" + p, ToClr(args[p]) ?? DBNull.Value);

            // 白名單可選參數
            if (def.OptionalParams is { Length: > 0 })
                foreach (var p in def.OptionalParams)
                    if (args.TryGetValue(p, out var v))
                        cmd.Parameters.AddWithValue("@" + p, ToClr(v) ?? DBNull.Value);

            var affected = await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
            return Ok(new { ok = true, rowsAffected = affected });
        }
        catch (Exception ex)
        {
            if (tx != null) { try { await tx.RollbackAsync(); } catch { } }
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }
}
