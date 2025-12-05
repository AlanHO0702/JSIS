using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;
using WebRazor.Models;

namespace PcbErpApi.Pages.CUR
{
    internal static class MasterDetailConfigLoader
    {
        public static async Task<MasterDetailLoadResult> LoadAsync(PcbErpContext ctx, ILogger logger, string itemId)
        {
            var result = new MasterDetailLoadResult { ItemId = itemId };

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

                var ocx = item.Ocxtemplete?.Trim() ?? string.Empty;
                if (item.ItemType != 6 || !ocx.Equals("JSdMasDtlDLL.dll", StringComparison.OrdinalIgnoreCase))
                {
                    result.Error = $"Item {itemId} is not a JSdMasDtlDLL master-detail item.";
                    result.IsNotFound = true;
                    return result;
                }

                var setups = await ctx.CurdOcxtableSetUp.AsNoTracking()
                    .Where(x => x.ItemId == itemId)
                    .ToListAsync();

                var master = setups.FirstOrDefault(x => string.Equals(x.TableKind, "MASTER1", StringComparison.OrdinalIgnoreCase));
                var detail = setups.FirstOrDefault(x => string.Equals(x.TableKind, "DETAIL1", StringComparison.OrdinalIgnoreCase));

                if (master == null || detail == null)
                {
                    result.Error = $"CURdOCXTableSetUp rows for MASTER1 / DETAIL1 are missing for item {itemId}.";
                    result.IsNotFound = true;
                    return result;
                }

                var connStr = GetConnStr(ctx);
                var masterMeta = await GetTableMetaAsync(connStr, master.TableName);
                var detailMeta = await GetTableMetaAsync(connStr, detail.TableName);

                var cfg = new MasterDetailConfig
                {
                    DomId = BuildDomId(itemId),
                    MasterTitle = masterMeta?.DisplayLabel ?? master.TableName,
                    DetailTitle = detailMeta?.DisplayLabel ?? detail.TableName,
                    MasterTable = masterMeta?.RealTableName ?? master.TableName,
                    DetailTable = detailMeta?.RealTableName ?? detail.TableName,
                    MasterDict = master.TableName,
                    DetailDict = detail.TableName,
                    KeyMap = BuildKeyMap(detail.Mdkey),
                    DetailKeyFields = Split(detail.LocateKeys),
                    MasterTop = 500
                };

                cfg.MasterOrderBy = string.IsNullOrWhiteSpace(master.OrderByField)
                    ? await GetDefaultOrderByAsync(connStr, cfg.MasterTable)
                    : master.OrderByField;

                result.Config = cfg;
                result.ItemName = item.ItemName;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Load master-detail config failed for {ItemId}", itemId);
                result.Error = ex.Message;
            }

            return result;
        }

        private static string BuildDomId(string itemId)
        {
            var safe = Regex.Replace(itemId ?? "md", "[^a-zA-Z0-9_-]", "_").Trim('_');
            if (string.IsNullOrWhiteSpace(safe))
                safe = "md";
            return $"md_{safe.ToLowerInvariant()}";
        }

        private static KeyMap[] BuildKeyMap(string? mdKey)
        {
            var keys = Split(mdKey);
            if (keys.Length == 0) return Array.Empty<KeyMap>();
            return keys.Select(k =>
            {
                var parts = k.Split(new[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);
                var master = parts.Length > 0 ? parts[0].Trim() : k;
                var detail = parts.Length > 1 ? parts[1].Trim() : master;
                master = string.IsNullOrWhiteSpace(master) ? k : master;
                detail = string.IsNullOrWhiteSpace(detail) ? master : detail;
                return new KeyMap(master, detail);
            }).ToArray();
        }

        private static string[] Split(string? raw)
        {
            return (raw ?? string.Empty)
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
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

        private static async Task<string> GetDefaultOrderByAsync(string connStr, string tableName)
        {
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"
SELECT TOP 1 c.name
  FROM sys.columns c
  JOIN sys.tables t ON t.object_id = c.object_id
 WHERE t.name = @tbl
 ORDER BY c.column_id";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", tableName ?? string.Empty);
            var result = await cmd.ExecuteScalarAsync();
            var col = result?.ToString();
            return string.IsNullOrWhiteSpace(col) ? "1" : $"[{col}]";
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

    internal sealed class MasterDetailLoadResult
    {
        public string ItemId { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public MasterDetailConfig? Config { get; set; }
        public string? Error { get; set; }
        public bool IsNotFound { get; set; }
    }
}
