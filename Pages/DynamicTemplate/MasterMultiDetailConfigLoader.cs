using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using WebRazor.Models;

namespace PcbErpApi.Pages.CUR
{
    internal static class MasterMultiDetailConfigLoader
    {
        public static async Task<MasterMultiDetailLoadResult> LoadAsync(PcbErpContext ctx, ILogger logger, string itemId)
        {
            var result = new MasterMultiDetailLoadResult { ItemId = itemId };

            try
            {
                var item = await ctx.CurdSysItems.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.ItemId == itemId);

                if (item is null)
                {
                    result.Error = $"Item {itemId} not found.";
                    result.IsNotFound = true;
                    return result;
                }

                var ocx = (item.Ocxtemplete ?? string.Empty).Trim();
                if (item.ItemType != 6 || !ocx.Equals("JSdMasMultiDtlDLL.dll", StringComparison.OrdinalIgnoreCase))
                {
                    result.Error = $"Item {itemId} is not a JSdMasMultiDtlDLL master-multi-detail item.";
                    result.IsNotFound = true;
                    return result;
                }

                var setups = await ctx.CurdOcxtableSetUp.AsNoTracking()
                    .Where(x => x.ItemId == itemId)
                    .ToListAsync();

                if (setups.Count == 0)
                {
                    result.Error = $"CURdOCXTableSetUp rows are missing for item {itemId}.";
                    result.IsNotFound = true;
                    return result;
                }

                var master = setups
                    .FirstOrDefault(x => (x.TableKind ?? string.Empty).Contains("MASTER", StringComparison.OrdinalIgnoreCase))
                    ?? setups.OrderBy(x => x.TableKind).FirstOrDefault();

                var details = setups
                    .Where(x => (x.TableKind ?? string.Empty).Contains("DETAIL", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => ExtractOrderIndex(x.TableKind))
                    .ThenBy(x => x.TableKind)
                    .ThenBy(x => x.TableName)
                    .ToList();

                if (master == null || details.Count == 0)
                {
                    result.Error = $"MASTER/DETAIL table setup not found for item {itemId}.";
                    result.IsNotFound = true;
                    return result;
                }

                var connStr = GetConnStr(ctx);
                var masterMeta = await GetTableMetaAsync(connStr, master.TableName);
                var masterTableName = masterMeta?.RealTableName ?? master.TableName;
                var masterPkFromSql = await GetPrimaryKeyColumnsAsync(connStr, masterTableName);
                var masterPkFields = Split(master.LocateKeys)
                    .Concat(masterPkFromSql)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var cfg = new MasterMultiDetailConfig
                {
                    DomId = BuildDomId(itemId),
                    ItemId = itemId,
                    MasterTitle = masterMeta?.DisplayLabel ?? master.TableName,
                    MasterTable = masterTableName,
                    MasterDict = master.TableName,
                    MasterApi = string.Empty,
                    MasterTop = 500,
                    MasterPkFields = masterPkFields
                };

                foreach (var d in details)
                {
                    var detailMeta = await GetTableMetaAsync(connStr, d.TableName);
                    var detailTableName = detailMeta?.RealTableName ?? d.TableName;
                    var pkFromSql = await GetPrimaryKeyColumnsAsync(connStr, detailTableName);
                    var pkFields = Split(d.LocateKeys)
                        .Concat(pkFromSql)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    cfg.Details.Add(new DetailConfig
                    {
                        DetailTitle = detailMeta?.DisplayLabel ?? d.TableName,
                        DetailTable = detailTableName,
                        DetailDict = d.TableName,
                        DetailApi = string.Empty,
                        KeyMap = BuildKeyMap(d.Mdkey),
                        PkFields = pkFields
                    });
                }

                result.Config = cfg;
                result.ItemName = item.ItemName;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Load master-multi-detail config failed for {ItemId}", itemId);
                result.Error = ex.Message;
            }

            return result;
        }

        private static string BuildDomId(string itemId)
        {
            var safe = Regex.Replace(itemId ?? "mmd", "[^a-zA-Z0-9_-]", "_").Trim('_');
            if (string.IsNullOrWhiteSpace(safe))
                safe = "mmd";
            return $"mmd_{safe.ToLowerInvariant()}";
        }

        private static int ExtractOrderIndex(string? tableKind)
        {
            if (string.IsNullOrWhiteSpace(tableKind)) return int.MaxValue;
            var m = Regex.Match(tableKind, "(\\d+)$");
            return m.Success && int.TryParse(m.Groups[1].Value, out var n) ? n : int.MaxValue;
        }

        private static List<KeyMapMulti> BuildKeyMap(string? mdKey)
        {
            var parts = Split(mdKey);
            if (parts.Count == 0) return new List<KeyMapMulti>();

            var list = new List<KeyMapMulti>(parts.Count);
            foreach (var k in parts)
            {
                var segs = k.Split(new[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);
                var master = segs.Length > 0 ? segs[0].Trim() : k.Trim();
                var detail = segs.Length > 1 ? segs[1].Trim() : master;
                if (string.IsNullOrWhiteSpace(master) || string.IsNullOrWhiteSpace(detail)) continue;
                list.Add(new KeyMapMulti { Master = master, Detail = detail });
            }
            return list;
        }

        private static List<string> Split(string? raw)
        {
            return (raw ?? string.Empty)
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        private static async Task<List<string>> GetPrimaryKeyColumnsAsync(string connStr, string tableName)
        {
            var shortName = NormalizeTableName(tableName);
            if (string.IsNullOrWhiteSpace(shortName)) return new List<string>();

            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"
SELECT c.name
  FROM sys.key_constraints kc
  JOIN sys.tables t ON t.object_id = kc.parent_object_id
  JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
  JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
 WHERE kc.type = 'PK'
   AND t.name = @tbl
 ORDER BY ic.key_ordinal;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", shortName);

            var list = new List<string>();
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var col = rd.GetString(0);
                if (!string.IsNullOrWhiteSpace(col))
                    list.Add(col);
            }
            return list;
        }

        private static string NormalizeTableName(string? raw)
        {
            var s = (raw ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            s = s.Replace("[", string.Empty).Replace("]", string.Empty);
            if (s.Contains('.'))
                s = s.Split('.').Last().Trim();
            return s;
        }

        private static async Task<TableMeta?> GetTableMetaAsync(string connStr, string dictTableName)
        {
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"
SELECT TOP 1 
    ISNULL(NULLIF(RealTableName,''), TableName) AS RealTableName,
    ISNULL(NULLIF(DisplayLabel,''), TableName) AS DisplayLabel
FROM CURdTableName WITH (NOLOCK)
WHERE TableName = @tbl";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", dictTableName ?? string.Empty);

            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rd.ReadAsync()) return null;

            return new TableMeta
            {
                RealTableName = rd["RealTableName"] == DBNull.Value ? null : rd["RealTableName"]?.ToString(),
                DisplayLabel = rd["DisplayLabel"] == DBNull.Value ? null : rd["DisplayLabel"]?.ToString()
            };
        }

        private static string GetConnStr(PcbErpContext ctx)
        {
            var cs = ctx.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string is not configured.");
            return cs;
        }

        private sealed class TableMeta
        {
            public string? RealTableName { get; set; }
            public string? DisplayLabel { get; set; }
        }
    }

    internal sealed class MasterMultiDetailLoadResult
    {
        public string ItemId { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public MasterMultiDetailConfig? Config { get; set; }
        public string? Error { get; set; }
        public bool IsNotFound { get; set; }
    }
}
