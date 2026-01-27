using System.Data;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UomGetLotController : ControllerBase
{
    private readonly string _cs;

    public UomGetLotController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? cfg.GetConnectionString("Default")
              ?? db?.Database.GetDbConnection().ConnectionString
              ?? throw new InvalidOperationException("Missing connection string");
    }

    public record InitRequest(string ItemId, string ButtonName, string PaperNum, string PaperId, string DetailItem);
    public record ConfirmRequest(string PaperNum, string PaperId, string DetailItem, int SpId);
    public record CountRequest(int SpId, string LotNum, string NeedQnty, string StockId, string PosId, int RoundToUom);
    public record SaveRowRequest(int SpId, string LotNum, string StockId, string PosId, int? InUse, decimal? TransQnty, decimal? TransUomQnty);
    public record AutoCountRequest(int SpId, string NeedQnty, string? StockId, string? PosId, string? Uom, string? SizeLenth, string? LotNum, int RoundToUom);

    [HttpPost("Init")]
    public async Task<IActionResult> Init([FromBody] InitRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum) || string.IsNullOrWhiteSpace(req.DetailItem))
            return BadRequest(new { ok = false, error = "PaperNum/DetailItem 為必填" });
        if (string.IsNullOrWhiteSpace(req.ButtonName))
            return BadRequest(new { ok = false, error = "ButtonName 為必填" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var (userId, useId) = await LoadUserContextAsync(conn, (SqlTransaction)tx);
            var paperId = await ResolveRealTableNameAsync(conn, (SqlTransaction)tx, req.PaperId) ?? req.PaperId;
            var systemId = await LoadSystemIdAsync(conn, (SqlTransaction)tx, req.ItemId);

            int spId;
            await using (var spCmd = new SqlCommand("select SPId=@@SPId", conn, (SqlTransaction)tx))
            {
                spId = Convert.ToInt32(await spCmd.ExecuteScalarAsync() ?? 0);
            }

            string partNum = string.Empty;
            string needQnty = string.Empty;
            await using (var cmd = new SqlCommand("exec MINdUOMGetLotIns @p0, @p1, @p2, @p3, @p4", conn, (SqlTransaction)tx))
            {
                cmd.Parameters.AddWithValue("@p0", paperId ?? string.Empty);
                cmd.Parameters.AddWithValue("@p1", req.PaperNum ?? string.Empty);
                cmd.Parameters.AddWithValue("@p2", req.DetailItem ?? string.Empty);
                cmd.Parameters.AddWithValue("@p3", spId);
                cmd.Parameters.AddWithValue("@p4", useId ?? string.Empty);

                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    partNum = rd["PartNum"]?.ToString() ?? string.Empty;
                    needQnty = rd["NeedQnty"]?.ToString() ?? string.Empty;
                }
            }

            var tranParams = await LoadTranParamsAsync(
                conn,
                (SqlTransaction)tx,
                req.ItemId,
                req.ButtonName,
                req.PaperNum ?? string.Empty,
                req.DetailItem ?? string.Empty,
                systemId,
                userId,
                useId ?? string.Empty);

            await tx.CommitAsync();
            return Ok(new { ok = true, spId, partNum, needQnty, tranParams });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    [HttpPost("Confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.PaperNum) || string.IsNullOrWhiteSpace(req.DetailItem) || req.SpId <= 0)
            return BadRequest(new { ok = false, error = "PaperNum/DetailItem/SpId 為必填" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var paperId = await ResolveRealTableNameAsync(conn, (SqlTransaction)tx, req.PaperId) ?? req.PaperId;
            await using var cmd = new SqlCommand("exec MINdUOMGetLotToDtl @p0, @p1, @p2, @p3", conn, (SqlTransaction)tx);
            cmd.Parameters.AddWithValue("@p0", paperId ?? string.Empty);
            cmd.Parameters.AddWithValue("@p1", req.PaperNum ?? string.Empty);
            cmd.Parameters.AddWithValue("@p2", req.DetailItem ?? string.Empty);
            cmd.Parameters.AddWithValue("@p3", req.SpId);
            await cmd.ExecuteNonQueryAsync();

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    [HttpGet("Rows")]
    public async Task<IActionResult> Rows(
        [FromQuery] int spId,
        [FromQuery] string? stockId,
        [FromQuery] string? posId,
        [FromQuery] string? uom,
        [FromQuery] string? sizeLenth,
        [FromQuery] string? lotNum)
    {
        if (spId <= 0) return BadRequest(new { ok = false, error = "SPId 為必填" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var (userId, _) = await LoadUserContextAsync(conn);
        var orderBy = await BuildOrderByAsync(conn, userId);

        var sql = "select * from MINdV_UOMGetLotQnty4Edit where SPId=@spId";
        var ps = new List<SqlParameter> { new("@spId", spId) };

        if (!string.IsNullOrWhiteSpace(stockId))
        {
            sql += " and StockId=@stockId";
            ps.Add(new SqlParameter("@stockId", stockId));
        }
        if (!string.IsNullOrWhiteSpace(posId))
        {
            sql += " and PosId=@posId";
            ps.Add(new SqlParameter("@posId", posId));
        }
        if (!string.IsNullOrWhiteSpace(uom))
        {
            sql += " and UOM=@uom";
            ps.Add(new SqlParameter("@uom", uom));
        }
        if (!string.IsNullOrWhiteSpace(sizeLenth))
        {
            sql += " and SizeLenth=@sizeLenth";
            ps.Add(new SqlParameter("@sizeLenth", sizeLenth));
        }
        if (!string.IsNullOrWhiteSpace(lotNum))
        {
            sql += " and BatchNum like @lotNum";
            ps.Add(new SqlParameter("@lotNum", "%" + lotNum + "%"));
        }
        if (!string.IsNullOrWhiteSpace(orderBy)) sql += orderBy;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(ps.ToArray());

        var list = new List<Dictionary<string, object?>>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
            list.Add(row);
        }

        return Ok(new { ok = true, rows = list });
    }

    [HttpGet("Sum")]
    public async Task<IActionResult> Sum([FromQuery] int spId)
    {
        if (spId <= 0) return BadRequest(new { ok = false, error = "SPId 為必填" });
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(@"
select sumQnty=sum(isnull(TransQnty,0))
from MINdUOMGetLotQnty (nolock)
where SPId=@spId and InUse=1", conn);
        cmd.Parameters.AddWithValue("@spId", spId);
        var obj = await cmd.ExecuteScalarAsync();
        return Ok(new { ok = true, sumQnty = obj == DBNull.Value ? 0 : obj });
    }

    [HttpPost("SaveRow")]
    public async Task<IActionResult> SaveRow([FromBody] SaveRowRequest req)
    {
        if (req == null || req.SpId <= 0 || string.IsNullOrWhiteSpace(req.LotNum))
            return BadRequest(new { ok = false, error = "SPId/LotNum 為必填" });

        var sets = new List<string>();
        var ps = new List<SqlParameter>();
        if (req.InUse != null)
        {
            sets.Add("InUse=@inUse");
            ps.Add(new SqlParameter("@inUse", req.InUse));
        }
        if (req.TransQnty != null)
        {
            sets.Add("TransQnty=@transQnty");
            ps.Add(new SqlParameter("@transQnty", req.TransQnty));
        }
        if (req.TransUomQnty != null)
        {
            sets.Add("TransUOMQnty=@transUomQnty");
            ps.Add(new SqlParameter("@transUomQnty", req.TransUomQnty));
        }
        if (sets.Count == 0)
            return Ok(new { ok = true, affected = 0 });

        var sql = $@"
update MINdUOMGetLotQnty
   set {string.Join(", ", sets)}
 where SPId=@spId
   and LotNum=@lotNum
   and isnull(StockId,'')=isnull(@stockId,'')
   and isnull(PosId,'')=isnull(@posId,'')";

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@spId", req.SpId);
        cmd.Parameters.AddWithValue("@lotNum", req.LotNum ?? string.Empty);
        cmd.Parameters.AddWithValue("@stockId", req.StockId ?? string.Empty);
        cmd.Parameters.AddWithValue("@posId", req.PosId ?? string.Empty);
        foreach (var p in ps) cmd.Parameters.Add(p);

        var affected = await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = true, affected });
    }

    [HttpPost("Count")]
    public async Task<IActionResult> Count([FromBody] CountRequest req)
    {
        if (req == null || req.SpId <= 0 || string.IsNullOrWhiteSpace(req.LotNum))
            return BadRequest(new { ok = false, error = "SPId/LotNum 為必填" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("exec MINdUOMGetLotCount @p0, @p1, @p2, @p3, @p4, @p5", conn);
        cmd.Parameters.AddWithValue("@p0", req.LotNum ?? string.Empty);
        cmd.Parameters.AddWithValue("@p1", req.SpId);
        cmd.Parameters.AddWithValue("@p2", req.NeedQnty ?? string.Empty);
        cmd.Parameters.AddWithValue("@p3", req.StockId ?? string.Empty);
        cmd.Parameters.AddWithValue("@p4", req.PosId ?? string.Empty);
        cmd.Parameters.AddWithValue("@p5", req.RoundToUom);
        await cmd.ExecuteNonQueryAsync();
        return Ok(new { ok = true });
    }

    [HttpPost("AutoCount")]
    public async Task<IActionResult> AutoCount([FromBody] AutoCountRequest req)
    {
        if (req == null || req.SpId <= 0)
            return BadRequest(new { ok = false, error = "SPId 為必填" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var (userId, _) = await LoadUserContextAsync(conn, (SqlTransaction)tx);
            var orderBy = await BuildOrderByAsync(conn, userId, (SqlTransaction)tx);

            await using (var resetCmd = new SqlCommand(@"
update MINdUOMGetLotQnty
   set InUse=0, TransQnty=0, TransUOMQnty=0
 where SPId=@spId", conn, (SqlTransaction)tx))
            {
                resetCmd.Parameters.AddWithValue("@spId", req.SpId);
                await resetCmd.ExecuteNonQueryAsync();
            }

            var sql = "select LotNum, StockId, PosId from MINdV_UOMGetLotQnty4Edit where SPId=@spId";
            var ps = new List<SqlParameter> { new("@spId", req.SpId) };
            if (!string.IsNullOrWhiteSpace(req.StockId))
            {
                sql += " and StockId=@stockId";
                ps.Add(new SqlParameter("@stockId", req.StockId));
            }
            if (!string.IsNullOrWhiteSpace(req.PosId))
            {
                sql += " and PosId=@posId";
                ps.Add(new SqlParameter("@posId", req.PosId));
            }
            if (!string.IsNullOrWhiteSpace(req.Uom))
            {
                sql += " and UOM=@uom";
                ps.Add(new SqlParameter("@uom", req.Uom));
            }
            if (!string.IsNullOrWhiteSpace(req.SizeLenth))
            {
                sql += " and SizeLenth=@sizeLenth";
                ps.Add(new SqlParameter("@sizeLenth", req.SizeLenth));
            }
            if (!string.IsNullOrWhiteSpace(req.LotNum))
            {
                sql += " and BatchNum like @lotNum";
                ps.Add(new SqlParameter("@lotNum", "%" + req.LotNum + "%"));
            }
            if (!string.IsNullOrWhiteSpace(orderBy)) sql += orderBy;

            var lots = new List<(string LotNum, string StockId, string PosId)>();
            await using (var selCmd = new SqlCommand(sql, conn, (SqlTransaction)tx))
            {
                selCmd.Parameters.AddRange(ps.ToArray());
                await using var rd = await selCmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    lots.Add((
                        rd["LotNum"]?.ToString() ?? string.Empty,
                        rd["StockId"]?.ToString() ?? string.Empty,
                        rd["PosId"]?.ToString() ?? string.Empty
                    ));
                }
            }

            foreach (var row in lots)
            {
                await using (var markCmd = new SqlCommand(@"
update MINdUOMGetLotQnty
   set InUse=1
 where SPId=@spId
   and LotNum=@lotNum
   and isnull(StockId,'')=isnull(@stockId,'')
   and isnull(PosId,'')=isnull(@posId,'')", conn, (SqlTransaction)tx))
                {
                    markCmd.Parameters.AddWithValue("@spId", req.SpId);
                    markCmd.Parameters.AddWithValue("@lotNum", row.LotNum);
                    markCmd.Parameters.AddWithValue("@stockId", row.StockId);
                    markCmd.Parameters.AddWithValue("@posId", row.PosId);
                    await markCmd.ExecuteNonQueryAsync();
                }

                await using (var countCmd = new SqlCommand("exec MINdUOMGetLotCount @p0, @p1, @p2, @p3, @p4, @p5", conn, (SqlTransaction)tx))
                {
                    countCmd.Parameters.AddWithValue("@p0", row.LotNum);
                    countCmd.Parameters.AddWithValue("@p1", req.SpId);
                    countCmd.Parameters.AddWithValue("@p2", req.NeedQnty ?? string.Empty);
                    countCmd.Parameters.AddWithValue("@p3", row.StockId);
                    countCmd.Parameters.AddWithValue("@p4", row.PosId);
                    countCmd.Parameters.AddWithValue("@p5", req.RoundToUom);
                    await countCmd.ExecuteNonQueryAsync();
                }

                decimal transUom = 0m;
                await using (var chkCmd = new SqlCommand(@"
select top 1 TransUOMQnty
from MINdV_UOMGetLotQnty4Edit
where SPId=@spId
  and LotNum=@lotNum
  and isnull(StockId,'')=isnull(@stockId,'')
  and isnull(PosId,'')=isnull(@posId,'')", conn, (SqlTransaction)tx))
                {
                    chkCmd.Parameters.AddWithValue("@spId", req.SpId);
                    chkCmd.Parameters.AddWithValue("@lotNum", row.LotNum);
                    chkCmd.Parameters.AddWithValue("@stockId", row.StockId);
                    chkCmd.Parameters.AddWithValue("@posId", row.PosId);
                    var obj = await chkCmd.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value)
                        decimal.TryParse(obj.ToString(), out transUom);
                }

                if (transUom == 0m)
                {
                    await using var unuseCmd = new SqlCommand(@"
update MINdUOMGetLotQnty
   set InUse=0
 where SPId=@spId
   and LotNum=@lotNum
   and isnull(StockId,'')=isnull(@stockId,'')
   and isnull(PosId,'')=isnull(@posId,'')", conn, (SqlTransaction)tx);
                    unuseCmd.Parameters.AddWithValue("@spId", req.SpId);
                    unuseCmd.Parameters.AddWithValue("@lotNum", row.LotNum);
                    unuseCmd.Parameters.AddWithValue("@stockId", row.StockId);
                    unuseCmd.Parameters.AddWithValue("@posId", row.PosId);
                    await unuseCmd.ExecuteNonQueryAsync();
                    break;
                }
            }

            await tx.CommitAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    [HttpGet("StockOptions")]
    public async Task<IActionResult> StockOptions()
    {
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        var (_, useId) = await LoadUserContextAsync(conn);

        var sql = @"
select StockId, StockName
from FMEdV_MINdStockBasic (nolock)
where (@useId='' or UseId=@useId)";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@useId", useId ?? string.Empty);

        var list = new List<object>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                StockId = rd["StockId"]?.ToString() ?? string.Empty,
                StockName = rd["StockName"]?.ToString() ?? string.Empty
            });
        }
        return Ok(new { ok = true, rows = list });
    }

    private static string? NormalizeIdentifier(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return name.Trim();
    }

    private async Task<string> BuildOrderByAsync(SqlConnection conn, string userId, SqlTransaction? tx = null)
    {
        var orderFields = new List<string>();
        await using var cmd = new SqlCommand("exec MINdUOMGetLotOrderBy @p0, @p1", conn, tx);
        cmd.Parameters.AddWithValue("@p0", "99999");
        cmd.Parameters.AddWithValue("@p1", userId ?? string.Empty);

        await using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
        {
            for (int i = 0; i < rd.FieldCount; i++)
            {
                var raw = rd.IsDBNull(i) ? "" : rd.GetValue(i)?.ToString();
                var name = NormalizeIdentifier(raw);
                if (string.IsNullOrWhiteSpace(name) || !IsValidIdentifierPart(name)) continue;
                orderFields.Add(string.Equals(name, "Ratio", StringComparison.OrdinalIgnoreCase)
                    ? $"{name} desc"
                    : name);
            }
        }
        return orderFields.Count == 0 ? string.Empty : " ORDER BY " + string.Join(", ", orderFields);
    }

    private async Task<(string UserId, string UseId)> LoadUserContextAsync(SqlConnection conn, SqlTransaction? tx = null)
    {
        var userId = string.Empty;
        var useId = string.Empty;

        var jwtHeader = Request?.Headers["X-JWTID"].ToString();
        if (!string.IsNullOrWhiteSpace(jwtHeader) && Guid.TryParse(jwtHeader, out var jwtId))
        {
            await using var cmd = new SqlCommand(
                "SELECT UserId FROM CURdUserOnline WITH (NOLOCK) WHERE JwtId = @jwtId", conn, tx);
            cmd.Parameters.AddWithValue("@jwtId", jwtId);
            userId = (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(userId))
            userId = User?.Identity?.Name ?? string.Empty;

        userId = userId.Trim();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await using var cmd = new SqlCommand(
                "SELECT UseId FROM CURdUsers WITH (NOLOCK) WHERE UserId = @userId", conn, tx);
            cmd.Parameters.AddWithValue("@userId", userId);
            useId = (await cmd.ExecuteScalarAsync())?.ToString() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(useId))
        {
            var claim =
                User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UseId", StringComparison.OrdinalIgnoreCase))
                ?? User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "useid", StringComparison.OrdinalIgnoreCase));
            useId = claim?.Value?.Trim() ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(useId)) useId = "A001";

        return (userId, useId);
    }

    private async Task<string?> LoadSystemIdAsync(SqlConnection conn, SqlTransaction? tx, string itemId)
    {
        const string sql = "SELECT TOP 1 SystemId FROM CURdSysItems WITH (NOLOCK) WHERE ItemId = @itemId;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, SqlTransaction tx, string? dictTableName)
    {
        if (string.IsNullOrWhiteSpace(dictTableName)) return null;
        const string sql = @"
select top 1 isnull(nullif(RealTableName,''), TableName) as ActualName
from CURdTableName (nolock)
where TableName=@tbl";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@tbl", dictTableName);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private sealed record TranParamRow(int SeqNum, string? TableKind, string? ParamFieldName, int ParamType);

    private async Task<List<object>> LoadTranParamsAsync(
        SqlConnection conn,
        SqlTransaction tx,
        string itemId,
        string buttonName,
        string paperNum,
        string detailItem,
        string? systemId,
        string userId,
        string useId)
    {
        var list = new List<TranParamRow>();
        const string sql = @"
SELECT SeqNum, TableKind, ParamFieldName, ISNULL(ParamType,0) AS ParamType
  FROM CURdOCXItmCusBtnTranPm WITH (NOLOCK)
 WHERE ItemId=@itemId AND ButtonName=@btn
 ORDER BY SeqNum, Seq;";
        await using (var cmd = new SqlCommand(sql, conn, tx))
        {
            cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
            cmd.Parameters.AddWithValue("@btn", buttonName ?? string.Empty);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new TranParamRow(
                    SeqNum: Convert.ToInt32(rd["SeqNum"] ?? 0),
                    TableKind: rd["TableKind"]?.ToString(),
                    ParamFieldName: rd["ParamFieldName"]?.ToString(),
                    ParamType: Convert.ToInt32(rd["ParamType"] ?? 0)
                ));
            }
        }

        if (list.Count == 0) return new List<object>();

        var tableMap = await LoadTableMapAsync(conn, tx, itemId ?? string.Empty);
        var result = new List<object>();
        foreach (var p in list)
        {
            object? value = p.ParamType switch
            {
                0 => await ReadFieldByKindAsync(
                    conn,
                    tx,
                    tableMap,
                    p.TableKind,
                    p.ParamFieldName,
                    paperNum ?? string.Empty,
                    detailItem ?? string.Empty),
                1 => p.ParamFieldName ?? string.Empty,
                2 => string.IsNullOrWhiteSpace(userId) ? string.Empty : userId,
                3 => string.IsNullOrWhiteSpace(useId) ? string.Empty : useId,
                4 => systemId ?? string.Empty,
                5 => paperNum ?? string.Empty,
                _ => p.ParamFieldName ?? string.Empty
            };
            result.Add(new { seqNum = p.SeqNum, value = value ?? string.Empty });
        }
        return result;
    }

    private async Task<Dictionary<string, string>> LoadTableMapAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        const string sql = @"
SELECT TableKind, TableName
  FROM CURdOCXTableSetUp WITH (NOLOCK)
 WHERE ItemId = @itemId;";
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@itemId", itemId ?? string.Empty);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var kind = (rd["TableKind"]?.ToString() ?? string.Empty).Trim();
            var table = rd["TableName"]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(kind) && !string.IsNullOrWhiteSpace(table) && !map.ContainsKey(kind))
                map[kind] = table;
        }
        return map;
    }

    private static string? ResolveTableName(Dictionary<string, string> tableMap, string? tableKind)
    {
        var kind = (tableKind ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(kind)) return null;
        if (tableMap.TryGetValue(kind, out var name)) return name;

        if (kind.StartsWith("Master", StringComparison.OrdinalIgnoreCase))
        {
            var master = tableMap.FirstOrDefault(x => x.Key.Contains("Master", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(master.Value) ? null : master.Value;
        }
        if (kind.StartsWith("Detail", StringComparison.OrdinalIgnoreCase))
        {
            var digit = new string(kind.SkipWhile(c => !char.IsDigit(c)).ToArray());
            if (!string.IsNullOrWhiteSpace(digit))
            {
                var match = tableMap.FirstOrDefault(x => x.Key.Equals($"Detail{digit}", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(match.Value)) return match.Value;
            }
            var firstDetail = tableMap.FirstOrDefault(x => x.Key.StartsWith("Detail", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(firstDetail.Value) ? null : firstDetail.Value;
        }
        if (kind.StartsWith("SubDetail", StringComparison.OrdinalIgnoreCase))
        {
            var digit = new string(kind.SkipWhile(c => !char.IsDigit(c)).ToArray());
            if (!string.IsNullOrWhiteSpace(digit))
            {
                var match = tableMap.FirstOrDefault(x => x.Key.Equals($"SubDetail{digit}", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(match.Value)) return match.Value;
            }
            var first = tableMap.FirstOrDefault(x => x.Key.StartsWith("SubDetail", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(first.Value) ? null : first.Value;
        }
        return null;
    }

    private async Task<object?> ReadFieldByKindAsync(
        SqlConnection conn,
        SqlTransaction tx,
        Dictionary<string, string> tableMap,
        string? tableKind,
        string? fieldName,
        string paperNum,
        string detailItem)
    {
        if (string.IsNullOrWhiteSpace(fieldName)) return null;
        var tableName = ResolveTableName(tableMap, tableKind);
        if (string.IsNullOrWhiteSpace(tableName)) return null;
        var actualTable = await ResolveRealTableNameAsync(conn, tx, tableName) ?? tableName;
        var safeTable = QuoteIdentifier(actualTable);
        var safeField = QuoteIdentifier(fieldName);
        if (string.IsNullOrWhiteSpace(safeTable) || string.IsNullOrWhiteSpace(safeField)) return null;

        var kind = (tableKind ?? string.Empty).Trim().ToLowerInvariant();
        var where = "WHERE [PaperNum] = @paperNum";
        if (kind.StartsWith("detail") || kind.StartsWith("subdetail"))
            where += " AND [Item] = @item";

        var sql = $"SELECT TOP 1 {safeField} FROM {safeTable} WITH (NOLOCK) {where}";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@paperNum", paperNum ?? string.Empty);
        if (kind.StartsWith("detail") || kind.StartsWith("subdetail"))
            cmd.Parameters.AddWithValue("@item", detailItem ?? string.Empty);

        var obj = await cmd.ExecuteScalarAsync();
        return obj == DBNull.Value ? null : obj;
    }

    private static bool IsValidIdentifierPart(string part)
    {
        return Regex.IsMatch(part, "^[A-Za-z_][A-Za-z0-9_]*$");
    }

    private static string QuoteIdentifier(string name)
    {
        var n = NormalizeIdentifier(name);
        if (string.IsNullOrWhiteSpace(n)) return string.Empty;
        var parts = n.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Any(p => !IsValidIdentifierPart(p))) return string.Empty;
        return string.Join(".", parts.Select(p => $"[{p}]"));
    }
}
