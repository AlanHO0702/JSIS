using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using WebRazor.Models;

namespace PcbErpApi.Pages.FQCdQCType
{
    public class IndexModel : PageModel
    {
        private readonly PcbErpContext _db;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(PcbErpContext db, ILogger<IndexModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        public MasterMultiDetailConfig Config { get; set; } = default!;
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            const string itemId = "FQC00002";

            try
            {
                // 從 CURdOCXTableSetUp 讀取 MASTER1 / DETAIL1 配置
                var setups = await _db.CurdOcxtableSetUp.AsNoTracking()
                    .Where(x => x.ItemId == itemId)
                    .ToListAsync();

                var masterSetup = setups.FirstOrDefault(x =>
                    string.Equals(x.TableKind, "MASTER1", StringComparison.OrdinalIgnoreCase));
                var detailSetup = setups.FirstOrDefault(x =>
                    string.Equals(x.TableKind, "DETAIL1", StringComparison.OrdinalIgnoreCase));

                if (masterSetup == null || detailSetup == null)
                {
                    ErrorMessage = $"CURdOCXTableSetUp 中找不到 {itemId} 的 MASTER1/DETAIL1 設定。";
                    return Page();
                }

                var connStr = _db.Database.GetConnectionString()!;

                // 從 CURdTableName 解析真實表名與顯示名稱
                var masterMeta = await GetTableMetaAsync(connStr, masterSetup.TableName);
                var detailMeta = await GetTableMetaAsync(connStr, detailSetup.TableName);

                var masterTable = masterMeta?.RealTableName ?? masterSetup.TableName;
                var detailTable = detailMeta?.RealTableName ?? detailSetup.TableName;

                // 解析 Master PK
                var masterPk = await GetPrimaryKeyColumnsAsync(connStr, masterTable);
                if (masterPk.Count == 0)
                    masterPk = Split(masterSetup.LocateKeys);

                // 解析 Detail PK
                var detailPk = await GetPrimaryKeyColumnsAsync(connStr, detailTable);
                if (detailPk.Count == 0)
                    detailPk = Split(detailSetup.LocateKeys);

                // 解析 Detail KeyMap (Mdkey)
                var detailKeyMap = BuildKeyMap(detailSetup.Mdkey);

                Config = new MasterMultiDetailConfig
                {
                    DomId = "fqcQCType",
                    ItemId = itemId,
                    MasterTitle = masterMeta?.DisplayLabel ?? masterSetup.TableName,
                    MasterTable = masterTable,
                    MasterDict = masterSetup.TableName,
                    MasterApi = "",
                    MasterTop = 99999,
                    MasterPkFields = masterPk,

                    Details = new List<DetailConfig>
                    {
                        // Detail[0]: 從 CURdOCXTableSetUp DETAIL1 配置讀取
                        new DetailConfig
                        {
                            DetailTitle = detailMeta?.DisplayLabel ?? detailSetup.TableName,
                            DetailTable = detailTable,
                            DetailDict = detailSetup.TableName,
                            DetailApi = "",
                            OrderByField = NormalizeOrderBy(detailSetup.OrderByField),
                            KeyMap = detailKeyMap,
                            PkFields = detailPk
                        },
                        // Detail[1]: SubDetail（Delphi 硬編碼的 FQCdQCTypeSubDtl）
                        new DetailConfig
                        {
                            DetailTitle = "檢驗子項",
                            DetailTable = "FQCdQCTypeSubDtl",
                            DetailDict = "FQCdQCTypeSubDtl",
                            DetailApi = "",
                            OrderByField = "SerialNum",
                            KeyMap = new List<KeyMapMulti>
                            {
                                new KeyMapMulti { Master = "QCType", Detail = "QCType" },
                                new KeyMapMulti { Master = "Item", Detail = "Item" }
                            },
                            PkFields = new List<string> { "QCType", "Item", "SerialNum" }
                        }
                    },

                    EnableDetailFocusCascade = true,
                    Layout = LayoutMode.Tabs,
                    EnableSplitters = false,
                    EnableGridCounts = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入 FQC00002 配置失敗");
                ErrorMessage = $"載入配置失敗: {ex.Message}";
            }

            return Page();
        }

        private static List<KeyMapMulti> BuildKeyMap(string? mdKey)
        {
            var parts = Split(mdKey);
            var list = new List<KeyMapMulti>();
            foreach (var k in parts)
            {
                var segs = k.Split(new[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);
                var master = segs.Length > 0 ? segs[0].Trim() : k.Trim();
                var detail = segs.Length > 1 ? segs[1].Trim() : master;
                if (!string.IsNullOrWhiteSpace(master) && !string.IsNullOrWhiteSpace(detail))
                    list.Add(new KeyMapMulti { Master = master, Detail = detail });
            }
            return list;
        }

        private static List<string> Split(string? raw)
        {
            return (raw ?? "")
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        private static string? NormalizeOrderBy(string? raw)
        {
            var s = (raw ?? "").Replace('*', ' ').Replace('+', ' ').Trim();
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        private static async Task<(string? RealTableName, string? DisplayLabel)?> GetTableMetaAsync(string connStr, string dictTableName)
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
            cmd.Parameters.AddWithValue("@tbl", dictTableName ?? "");
            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rd.ReadAsync()) return null;
            return (
                rd["RealTableName"] == DBNull.Value ? null : rd["RealTableName"]?.ToString(),
                rd["DisplayLabel"] == DBNull.Value ? null : rd["DisplayLabel"]?.ToString()
            );
        }

        private static async Task<List<string>> GetPrimaryKeyColumnsAsync(string connStr, string tableName)
        {
            var tbl = (tableName ?? "").Replace("[", "").Replace("]", "").Trim();
            if (tbl.Contains('.')) tbl = tbl.Split('.').Last().Trim();
            if (string.IsNullOrWhiteSpace(tbl)) return new List<string>();

            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            const string sql = @"
                SELECT c.name
                FROM sys.key_constraints kc
                JOIN sys.tables t ON t.object_id = kc.parent_object_id
                JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
                JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                WHERE kc.type = 'PK' AND t.name = @tbl
                ORDER BY ic.key_ordinal";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tbl", tbl);
            var list = new List<string>();
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var col = rd.GetString(0);
                if (!string.IsNullOrWhiteSpace(col)) list.Add(col);
            }
            return list;
        }
    }
}
