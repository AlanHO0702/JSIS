using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

[ApiController]
[Route("api/[controller]")]
public class PO000015OutCostSpareController : ControllerBase
{
    private readonly string _cs;

    public PO000015OutCostSpareController(IConfiguration cfg, PcbErpContext db)
    {
        _cs = cfg.GetConnectionString("DefaultConnection")
              ?? cfg.GetConnectionString("Default")
              ?? db.Database.GetDbConnection().ConnectionString;
    }

    public class InitRequest
    {
        public string itemId { get; set; } = "";
        public string buttonName { get; set; } = "";
        public string paperNum { get; set; } = "";
        public string detailItem { get; set; } = "";
    }

    public class SaveRequest
    {
        public int iType { get; set; }
        public string paperNum { get; set; } = "";
        public int iItem { get; set; }
        public int iAllItem { get; set; }
        public List<CostRowDto> rows { get; set; } = new();
    }

    public class CostRowDto
    {
        public int? Item { get; set; }
        public int? SourItem { get; set; }
        public string? CompanyId { get; set; }
        public int? ChargeType { get; set; }
        public decimal? Charge2 { get; set; }
        public decimal? Charge { get; set; }
        public decimal? SubTotal { get; set; }
        public int? AssessedType { get; set; }
        public string? InvoiceNum { get; set; }
        public int? IsCome { get; set; }
    }

    [HttpPost("Init")]
    public async Task<IActionResult> Init([FromBody] InitRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.itemId) || string.IsNullOrWhiteSpace(req.buttonName))
            return Ok(new { ok = false, error = "ItemId/ButtonName 不可空白" });

        var reqPaperNum = (req.paperNum ?? "").Trim();
        if (string.IsNullOrWhiteSpace(reqPaperNum))
            return Ok(new { ok = false, error = "單號不可空白" });

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var (userId, useId) = await LoadUserContextAsync(conn, (SqlTransaction)tx);
            var systemId = await LoadSystemIdAsync(conn, (SqlTransaction)tx, req.itemId);
            var tranParams = await LoadTranParamsAsync(
                conn,
                (SqlTransaction)tx,
                req.itemId,
                req.buttonName,
                reqPaperNum,
                (req.detailItem ?? "").Trim(),
                systemId,
                userId,
                useId);

            var tranMap = tranParams.ToDictionary(x => x.SeqNum, x => x.Value ?? "", EqualityComparer<int>.Default);
            var iType = ParseInt(tranMap.TryGetValue(1, out var v1) ? v1 : "0", 0);
            var nowPaperNum = (tranMap.TryGetValue(2, out var v2) ? v2 : reqPaperNum).Trim();
            var iItem = ParseInt(tranMap.TryGetValue(3, out var v3) ? v3 : req.detailItem, 0);
            var allRaw = (tranMap.TryGetValue(4, out var v4) ? v4 : "0").Trim();
            var iAllItem = allRaw is "0" or "1" ? ParseInt(allRaw, 0) : 0;

            var chargeTypes = await LoadChargeTypesAsync(conn, (SqlTransaction)tx);
            var customers = new List<object>();
            var assessedTypes = new List<object>();
            var rows = new List<CostRowDto>();

            if (iType == 0)
            {
                customers = await LoadCustomersAsync(conn, (SqlTransaction)tx);
                assessedTypes = await LoadAssessedTypesAsync(conn, (SqlTransaction)tx);
                rows = await LoadOutAssessedRowsAsync(conn, (SqlTransaction)tx, nowPaperNum);
            }
            else if (iType == 1)
            {
                var partNum = await LoadDetailPartNumAsync(conn, (SqlTransaction)tx, "MPHdOrderSub", nowPaperNum, iItem);
                if (!string.IsNullOrWhiteSpace(partNum))
                {
                    await using var cmd = new SqlCommand("exec MPHdBudgetSetData @SourNum, @SourItem, @iAllItem", conn, (SqlTransaction)tx);
                    cmd.Parameters.AddWithValue("@SourNum", nowPaperNum);
                    cmd.Parameters.AddWithValue("@SourItem", iItem);
                    cmd.Parameters.AddWithValue("@iAllItem", iAllItem);
                    await cmd.ExecuteNonQueryAsync();
                }
                rows = await LoadBudgetRowsAsync(conn, (SqlTransaction)tx, "MPHdBudget", nowPaperNum, iItem, isSPO: false);
            }
            else
            {
                var exists = await ExistsDetailRowAsync(conn, (SqlTransaction)tx, "SPOdOrderSub", nowPaperNum, iItem);
                if (exists)
                {
                    await using var cmd = new SqlCommand("exec SPOdBudgetSetData @SourNum, @SourItem, @iAllItem", conn, (SqlTransaction)tx);
                    cmd.Parameters.AddWithValue("@SourNum", nowPaperNum);
                    cmd.Parameters.AddWithValue("@SourItem", iItem);
                    cmd.Parameters.AddWithValue("@iAllItem", iAllItem);
                    await cmd.ExecuteNonQueryAsync();
                }
                rows = await LoadBudgetRowsAsync(conn, (SqlTransaction)tx, "SPOdBudget", nowPaperNum, iItem, isSPO: true);
            }

            await tx.CommitAsync();
            return Ok(new
            {
                ok = true,
                data = new
                {
                    iType,
                    paperNum = nowPaperNum,
                    iItem,
                    iAllItem,
                    rows
                },
                lookups = new
                {
                    chargeTypes,
                    customers,
                    assessedTypes
                }
            });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Ok(new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    [HttpPost("Save")]
    public async Task<IActionResult> Save([FromBody] SaveRequest req)
    {
        var paperNum = (req.paperNum ?? "").Trim();
        if (string.IsNullOrWhiteSpace(paperNum))
            return Ok(new { ok = false, error = "單號不可空白" });

        var rows = req.rows ?? new List<CostRowDto>();
        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            if (req.iType == 0)
            {
                await using (var del = new SqlCommand("delete from MPHdOutAssessed where PaperNum=@PaperNum", conn, (SqlTransaction)tx))
                {
                    del.Parameters.AddWithValue("@PaperNum", paperNum);
                    await del.ExecuteNonQueryAsync();
                }

                var ordered = rows.OrderBy(r => r.Item ?? int.MaxValue).ToList();
                var nextItem = 1;
                foreach (var r in ordered)
                {
                    var item = (r.Item ?? 0) > 0 ? (r.Item ?? 0) : nextItem;
                    nextItem = Math.Max(nextItem, item + 1);
                    await using var ins = new SqlCommand(@"
insert into MPHdOutAssessed (PaperNum, Item, CompanyId, ChargeType, SubTotal, AssessedType, InvoiceNum)
values (@PaperNum, @Item, @CompanyId, @ChargeType, @SubTotal, @AssessedType, @InvoiceNum)", conn, (SqlTransaction)tx);
                    ins.Parameters.AddWithValue("@PaperNum", paperNum);
                    ins.Parameters.AddWithValue("@Item", item);
                    ins.Parameters.AddWithValue("@CompanyId", (object?)r.CompanyId?.Trim() ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@ChargeType", (object?)r.ChargeType ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@SubTotal", (object?)r.SubTotal ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@AssessedType", (object?)r.AssessedType ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@InvoiceNum", (object?)r.InvoiceNum?.Trim() ?? DBNull.Value);
                    await ins.ExecuteNonQueryAsync();
                }
            }
            else if (req.iType == 1)
            {
                await ReplaceBudgetRowsAsync(conn, (SqlTransaction)tx, "MPHdBudget", paperNum, req.iItem, rows, isSPO: false);
                await using var cmd = new SqlCommand("exec MPHdBudgetPost @SourNum, @SourItem, @iAllItem", conn, (SqlTransaction)tx);
                cmd.Parameters.AddWithValue("@SourNum", paperNum);
                cmd.Parameters.AddWithValue("@SourItem", req.iItem);
                cmd.Parameters.AddWithValue("@iAllItem", req.iAllItem);
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                await ReplaceBudgetRowsAsync(conn, (SqlTransaction)tx, "SPOdBudget", paperNum, req.iItem, rows, isSPO: true);
                await using var cmd = new SqlCommand("exec SPOdBudgetPost @SourNum, @SourItem, @iAllItem", conn, (SqlTransaction)tx);
                cmd.Parameters.AddWithValue("@SourNum", paperNum);
                cmd.Parameters.AddWithValue("@SourItem", req.iItem);
                cmd.Parameters.AddWithValue("@iAllItem", req.iAllItem);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();

            var message = req.iType switch
            {
                1 => "重新計算分期付款完成！",
                2 => "分期付款計算完成！",
                _ => "儲存完成"
            };
            return Ok(new { ok = true, message });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Ok(new { ok = false, error = ex.GetBaseException().Message });
        }
    }

    private async Task ReplaceBudgetRowsAsync(
        SqlConnection conn,
        SqlTransaction tx,
        string tableName,
        string sourNum,
        int sourItem,
        List<CostRowDto> rows,
        bool isSPO)
    {
        var safeTable = QuoteIdentifier(tableName);
        if (string.IsNullOrWhiteSpace(safeTable))
            throw new InvalidOperationException("Invalid table name");

        await using (var del = new SqlCommand($"delete from {safeTable} where SourNum=@SourNum and SourItem=@SourItem", conn, tx))
        {
            del.Parameters.AddWithValue("@SourNum", sourNum);
            del.Parameters.AddWithValue("@SourItem", sourItem);
            await del.ExecuteNonQueryAsync();
        }

        var ordered = (rows ?? new List<CostRowDto>()).OrderBy(r => r.Item ?? int.MaxValue).ToList();
        var nextItem = 1;
        foreach (var r in ordered)
        {
            var item = (r.Item ?? 0) > 0 ? (r.Item ?? 0) : nextItem;
            nextItem = Math.Max(nextItem, item + 1);

            if (isSPO)
            {
                await using var ins = new SqlCommand($@"
insert into {safeTable} (SourNum, SourItem, Item, ChargeType, Charge, IsCome)
values (@SourNum, @SourItem, @Item, @ChargeType, @Charge, @IsCome)", conn, tx);
                ins.Parameters.AddWithValue("@SourNum", sourNum);
                ins.Parameters.AddWithValue("@SourItem", sourItem);
                ins.Parameters.AddWithValue("@Item", item);
                ins.Parameters.AddWithValue("@ChargeType", (object?)r.ChargeType ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Charge", (object?)r.Charge ?? DBNull.Value);
                ins.Parameters.AddWithValue("@IsCome", (object?)r.IsCome ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync();
            }
            else
            {
                await using var ins = new SqlCommand($@"
insert into {safeTable} (SourNum, SourItem, Item, ChargeType, Charge2, Charge, IsCome)
values (@SourNum, @SourItem, @Item, @ChargeType, @Charge2, @Charge, @IsCome)", conn, tx);
                ins.Parameters.AddWithValue("@SourNum", sourNum);
                ins.Parameters.AddWithValue("@SourItem", sourItem);
                ins.Parameters.AddWithValue("@Item", item);
                ins.Parameters.AddWithValue("@ChargeType", (object?)r.ChargeType ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Charge2", (object?)r.Charge2 ?? DBNull.Value);
                ins.Parameters.AddWithValue("@Charge", (object?)r.Charge ?? DBNull.Value);
                ins.Parameters.AddWithValue("@IsCome", (object?)r.IsCome ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task<List<object>> LoadChargeTypesAsync(SqlConnection conn, SqlTransaction tx)
    {
        var list = new List<object>();
        await using var cmd = new SqlCommand(@"
select ChargeType, ChargeTypeName
from MPHdChargeType with(nolock)
order by ChargeType", conn, tx);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                chargeType = rd["ChargeType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["ChargeType"]),
                chargeTypeName = rd["ChargeTypeName"]?.ToString() ?? ""
            });
        }
        return list;
    }

    private async Task<List<object>> LoadCustomersAsync(SqlConnection conn, SqlTransaction tx)
    {
        var list = new List<object>();
        await using var cmd = new SqlCommand(@"
select CustomerId=CompanyId, ShortName
from AJNdMTLSupplier with(nolock)
order by CompanyId", conn, tx);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                customerId = rd["CustomerId"]?.ToString() ?? "",
                shortName = rd["ShortName"]?.ToString() ?? ""
            });
        }
        return list;
    }

    private async Task<List<object>> LoadAssessedTypesAsync(SqlConnection conn, SqlTransaction tx)
    {
        var list = new List<object>();
        await using var cmd = new SqlCommand(@"
select AssessedType, AssessedName
from MPHdAssessedType with(nolock)
order by AssessedType", conn, tx);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new
            {
                assessedType = rd["AssessedType"] == DBNull.Value ? 0 : Convert.ToInt32(rd["AssessedType"]),
                assessedName = rd["AssessedName"]?.ToString() ?? ""
            });
        }
        return list;
    }

    private async Task<List<CostRowDto>> LoadOutAssessedRowsAsync(SqlConnection conn, SqlTransaction tx, string paperNum)
    {
        var list = new List<CostRowDto>();
        await using var cmd = new SqlCommand(@"
select Item, CompanyId, ChargeType, SubTotal, AssessedType, InvoiceNum
from MPHdOutAssessed with(nolock)
where PaperNum=@PaperNum
order by Item", conn, tx);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum ?? "");
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new CostRowDto
            {
                Item = rd["Item"] == DBNull.Value ? null : Convert.ToInt32(rd["Item"]),
                CompanyId = rd["CompanyId"]?.ToString() ?? "",
                ChargeType = rd["ChargeType"] == DBNull.Value ? null : Convert.ToInt32(rd["ChargeType"]),
                SubTotal = rd["SubTotal"] == DBNull.Value ? null : Convert.ToDecimal(rd["SubTotal"]),
                AssessedType = rd["AssessedType"] == DBNull.Value ? null : Convert.ToInt32(rd["AssessedType"]),
                InvoiceNum = rd["InvoiceNum"]?.ToString() ?? ""
            });
        }
        return list;
    }

    private async Task<List<CostRowDto>> LoadBudgetRowsAsync(SqlConnection conn, SqlTransaction tx, string tableName, string sourNum, int sourItem, bool isSPO)
    {
        var safeTable = QuoteIdentifier(tableName);
        if (string.IsNullOrWhiteSpace(safeTable)) return new List<CostRowDto>();

        var sql = isSPO
            ? $@"select SourItem, Item, ChargeType, Charge, IsCome
                from {safeTable} with(nolock)
                where SourNum=@SourNum and SourItem=@SourItem
                order by Item"
            : $@"select Item, SourItem, ChargeType, Charge2, Charge, IsCome
                from {safeTable} with(nolock)
                where SourNum=@SourNum and SourItem=@SourItem
                order by Item";

        var list = new List<CostRowDto>();
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@SourNum", sourNum ?? "");
        cmd.Parameters.AddWithValue("@SourItem", sourItem);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            list.Add(new CostRowDto
            {
                Item = rd["Item"] == DBNull.Value ? null : Convert.ToInt32(rd["Item"]),
                SourItem = rd["SourItem"] == DBNull.Value ? null : Convert.ToInt32(rd["SourItem"]),
                ChargeType = rd["ChargeType"] == DBNull.Value ? null : Convert.ToInt32(rd["ChargeType"]),
                Charge2 = TryReadDecimal(rd, "Charge2"),
                Charge = TryReadDecimal(rd, "Charge"),
                IsCome = rd["IsCome"] == DBNull.Value ? null : Convert.ToInt32(rd["IsCome"])
            });
        }
        return list;
    }

    private static decimal? TryReadDecimal(SqlDataReader rd, string name)
    {
        try
        {
            var obj = rd[name];
            return obj == DBNull.Value ? null : Convert.ToDecimal(obj);
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> LoadDetailPartNumAsync(SqlConnection conn, SqlTransaction tx, string tableName, string paperNum, int item)
    {
        var safeTable = QuoteIdentifier(tableName);
        if (string.IsNullOrWhiteSpace(safeTable)) return "";
        await using var cmd = new SqlCommand($@"
select top 1 PartNum
from {safeTable} with(nolock)
where PaperNum=@PaperNum and Item=@Item", conn, tx);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum ?? "");
        cmd.Parameters.AddWithValue("@Item", item);
        return (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
    }

    private async Task<bool> ExistsDetailRowAsync(SqlConnection conn, SqlTransaction tx, string tableName, string paperNum, int item)
    {
        var safeTable = QuoteIdentifier(tableName);
        if (string.IsNullOrWhiteSpace(safeTable)) return false;
        await using var cmd = new SqlCommand($@"
select top 1 Item
from {safeTable} with(nolock)
where PaperNum=@PaperNum and Item=@Item", conn, tx);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum ?? "");
        cmd.Parameters.AddWithValue("@Item", item);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null && obj != DBNull.Value;
    }

    private sealed record TranParamRow(int SeqNum, string? Value);
    private sealed record TranParamDef(int SeqNum, string? TableKind, string? ParamFieldName, int ParamType);

    private async Task<List<TranParamRow>> LoadTranParamsAsync(
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
        var defs = new List<TranParamDef>();
        await using (var cmd = new SqlCommand(@"
select SeqNum, TableKind, ParamFieldName, isnull(ParamType,0) as ParamType
from CURdOCXItmCusBtnTranPm with(nolock)
where ItemId=@ItemId and ButtonName=@ButtonName
order by SeqNum, Seq", conn, tx))
        {
            cmd.Parameters.AddWithValue("@ItemId", itemId ?? "");
            cmd.Parameters.AddWithValue("@ButtonName", buttonName ?? "");
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                defs.Add(new TranParamDef(
                    SeqNum: Convert.ToInt32(rd["SeqNum"] ?? 0),
                    TableKind: rd["TableKind"]?.ToString(),
                    ParamFieldName: rd["ParamFieldName"]?.ToString(),
                    ParamType: Convert.ToInt32(rd["ParamType"] ?? 0)));
            }
        }

        if (defs.Count == 0) return new List<TranParamRow>();

        var tableMap = await LoadTableMapAsync(conn, tx, itemId ?? string.Empty);
        var list = new List<TranParamRow>();
        foreach (var d in defs)
        {
            object? value = d.ParamType switch
            {
                0 => await ReadFieldByKindAsync(conn, tx, tableMap, d.TableKind, d.ParamFieldName, paperNum, detailItem),
                1 => d.ParamFieldName ?? string.Empty,
                2 => userId,
                3 => useId,
                4 => systemId ?? string.Empty,
                5 => paperNum,
                _ => d.ParamFieldName ?? string.Empty
            };
            list.Add(new TranParamRow(d.SeqNum, value?.ToString() ?? ""));
        }
        return list;
    }

    private async Task<Dictionary<string, string>> LoadTableMapAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var cmd = new SqlCommand(@"
select TableKind, TableName
from CURdOCXTableSetUp with(nolock)
where ItemId=@ItemId", conn, tx);
        cmd.Parameters.AddWithValue("@ItemId", itemId ?? "");
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var kind = (rd["TableKind"]?.ToString() ?? "").Trim();
            var table = (rd["TableName"]?.ToString() ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(kind) && !string.IsNullOrWhiteSpace(table) && !map.ContainsKey(kind))
                map[kind] = table;
        }
        return map;
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
        var actual = await ResolveRealTableNameAsync(conn, tx, tableName) ?? tableName;
        var safeTable = QuoteIdentifier(actual);
        var safeField = QuoteIdentifier(fieldName);
        if (string.IsNullOrWhiteSpace(safeTable) || string.IsNullOrWhiteSpace(safeField)) return null;

        var kind = (tableKind ?? "").Trim().ToLowerInvariant();
        var where = "where PaperNum=@PaperNum";
        var needItem = kind.StartsWith("detail") || kind.StartsWith("subdetail");
        if (needItem) where += " and Item=@Item";

        await using var cmd = new SqlCommand($"select top 1 {safeField} from {safeTable} with(nolock) {where}", conn, tx);
        cmd.Parameters.AddWithValue("@PaperNum", paperNum ?? "");
        if (needItem) cmd.Parameters.AddWithValue("@Item", detailItem ?? "");
        var obj = await cmd.ExecuteScalarAsync();
        return obj == DBNull.Value ? null : obj;
    }

    private static string? ResolveTableName(Dictionary<string, string> tableMap, string? tableKind)
    {
        var kind = (tableKind ?? "").Trim();
        if (string.IsNullOrWhiteSpace(kind)) return null;
        if (tableMap.TryGetValue(kind, out var hit)) return hit;

        if (kind.StartsWith("Master", StringComparison.OrdinalIgnoreCase))
        {
            var m = tableMap.FirstOrDefault(x => x.Key.Contains("Master", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(m.Value) ? null : m.Value;
        }
        if (kind.StartsWith("Detail", StringComparison.OrdinalIgnoreCase))
        {
            var digit = new string(kind.SkipWhile(c => !char.IsDigit(c)).ToArray());
            if (!string.IsNullOrWhiteSpace(digit))
            {
                var m = tableMap.FirstOrDefault(x => x.Key.Equals($"Detail{digit}", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(m.Value)) return m.Value;
            }
            var d = tableMap.FirstOrDefault(x => x.Key.StartsWith("Detail", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(d.Value) ? null : d.Value;
        }
        if (kind.StartsWith("SubDetail", StringComparison.OrdinalIgnoreCase))
        {
            var d = tableMap.FirstOrDefault(x => x.Key.StartsWith("SubDetail", StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(d.Value) ? null : d.Value;
        }
        return null;
    }

    private async Task<string?> LoadSystemIdAsync(SqlConnection conn, SqlTransaction tx, string itemId)
    {
        await using var cmd = new SqlCommand(
            "select top 1 SystemId from CURdSysItems with(nolock) where ItemId=@ItemId", conn, tx);
        cmd.Parameters.AddWithValue("@ItemId", itemId ?? "");
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private async Task<(string UserId, string UseId)> LoadUserContextAsync(SqlConnection conn, SqlTransaction tx)
    {
        var userId = "";
        var useId = "";

        var jwtHeader = Request?.Headers["X-JWTID"].ToString();
        if (!string.IsNullOrWhiteSpace(jwtHeader) && Guid.TryParse(jwtHeader, out var jwtId))
        {
            await using var cmd = new SqlCommand(
                "select UserId from CURdUserOnline with(nolock) where JwtId=@JwtId", conn, tx);
            cmd.Parameters.AddWithValue("@JwtId", jwtId);
            userId = (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
        }

        if (string.IsNullOrWhiteSpace(userId))
            userId = User?.Identity?.Name ?? "";
        userId = userId.Trim();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await using var cmd = new SqlCommand(
                "select UseId from CURdUsers with(nolock) where UserId=@UserId", conn, tx);
            cmd.Parameters.AddWithValue("@UserId", userId);
            useId = (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
        }

        if (string.IsNullOrWhiteSpace(useId))
        {
            useId =
                User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UseId", StringComparison.OrdinalIgnoreCase))?.Value
                ?? User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "useid", StringComparison.OrdinalIgnoreCase))?.Value
                ?? "";
        }
        if (string.IsNullOrWhiteSpace(useId)) useId = "A001";

        return (userId, useId);
    }

    private async Task<string?> ResolveRealTableNameAsync(SqlConnection conn, SqlTransaction tx, string? dictTableName)
    {
        if (string.IsNullOrWhiteSpace(dictTableName)) return null;
        await using var cmd = new SqlCommand(@"
select top 1 isnull(nullif(RealTableName,''), TableName) as ActualName
from CURdTableName with(nolock)
where TableName=@TableName", conn, tx);
        cmd.Parameters.AddWithValue("@TableName", dictTableName);
        var obj = await cmd.ExecuteScalarAsync();
        return obj == null || obj == DBNull.Value ? null : obj.ToString();
    }

    private static int ParseInt(string? text, int defaultValue)
    {
        return int.TryParse((text ?? "").Trim(), out var n) ? n : defaultValue;
    }

    private static bool IsValidIdentifierPart(string part)
    {
        return Regex.IsMatch(part, "^[A-Za-z_][A-Za-z0-9_]*$");
    }

    private static string QuoteIdentifier(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var parts = name.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Any(p => !IsValidIdentifierPart(p))) return string.Empty;
        return string.Join(".", parts.Select(p => $"[{p}]"));
    }
}
