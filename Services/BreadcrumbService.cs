using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Services
{
    public interface IBreadcrumbService
    {
        Task<List<BreadcrumbItem>> BuildBreadcrumbsAsync(string? superId);
    }

    public class BreadcrumbService : IBreadcrumbService
    {
        private readonly PcbErpContext _ctx;

        public BreadcrumbService(PcbErpContext ctx)
        {
            _ctx = ctx;
        }

        /// <summary>
        /// 建立麵包屑導航，從指定的 SuperId 向上追溯到 Level 0
        /// </summary>
        public async Task<List<BreadcrumbItem>> BuildBreadcrumbsAsync(string? superId)
        {
            var breadcrumbs = new List<BreadcrumbItem>();
            if (string.IsNullOrWhiteSpace(superId)) return breadcrumbs;

            var cs = _ctx.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(cs)) return breadcrumbs;

            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            var currentSuperId = superId;
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (!string.IsNullOrWhiteSpace(currentSuperId) && !visited.Contains(currentSuperId))
            {
                visited.Add(currentSuperId);

                const string sql = @"
SELECT ItemId, ItemName, SuperId, LevelNo
  FROM CURdSysItems WITH (NOLOCK)
 WHERE ItemId = @itemId";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@itemId", currentSuperId);

                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    var itemId = rd["ItemId"]?.ToString() ?? "";
                    var itemName = rd["ItemName"]?.ToString() ?? "";
                    var nextSuperId = rd["SuperId"]?.ToString();
                    var levelNo = rd["LevelNo"] == DBNull.Value ? -1 : Convert.ToInt32(rd["LevelNo"]);

                    // Level 1 可以點擊回到 FontIndex
                    string? url = levelNo == 1 ? $"/FontIndex?level1Id={itemId}" : null;

                    breadcrumbs.Insert(0, new BreadcrumbItem
                    {
                        Id = itemId,
                        Name = itemName,
                        Url = url
                    });

                    currentSuperId = nextSuperId;
                }
                else
                {
                    break;
                }
            }

            return breadcrumbs;
        }
    }
}
