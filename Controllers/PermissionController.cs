using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PcbErpApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcbErpApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly string _connStr;

        public PermissionController(PcbErpContext context, IConfiguration config)
        {
            var dbConnStr = context.Database.GetDbConnection().ConnectionString;
            _connStr = config["ConnectionStrings:DefaultConnection"]
                ?? dbConnStr
                ?? throw new InvalidOperationException("Missing connection string.");
        }

        [HttpGet("ItemAccess")]
        public async Task<IActionResult> GetItemAccess([FromQuery] string userId, [FromQuery] string itemId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(itemId))
                return BadRequest("userId and itemId are required.");

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            var grpIds = new List<string>();
            await using (var cmd = new SqlCommand("SELECT GroupId FROM CURdUserGroup WITH (NOLOCK) WHERE UserId=@uid", conn))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var gid = rd["GroupId"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(gid)) grpIds.Add(gid);
                }
            }

            int grpUpdateMoney = 0, grpUpdateNotes = 0;
            if (grpIds.Count > 0)
            {
                var inClause = string.Join(",", grpIds.Select((_, i) => $"@g{i}"));
                var sql = $@"
SELECT
  MAX(ISNULL(bUpdateMoney,0)) AS bUpdateMoney,
  MAX(ISNULL(bUpdateNotes,0)) AS bUpdateNotes
FROM CURdGroupItems WITH (NOLOCK)
WHERE ItemId=@itemId AND GroupId IN ({inClause})";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@itemId", itemId);
                for (var i = 0; i < grpIds.Count; i++)
                    cmd.Parameters.AddWithValue($"@g{i}", grpIds[i]);
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    grpUpdateMoney = rd["bUpdateMoney"] == DBNull.Value ? 0 : Convert.ToInt32(rd["bUpdateMoney"]);
                    grpUpdateNotes = rd["bUpdateNotes"] == DBNull.Value ? 0 : Convert.ToInt32(rd["bUpdateNotes"]);
                }
            }

            int userUpdateMoney = 0, userUpdateNotes = 0, userRowCount = 0;
            await using (var cmd = new SqlCommand(@"
SELECT
  COUNT(1) AS Cnt,
  MAX(ISNULL(bUpdateMoney,0)) AS bUpdateMoney,
  MAX(ISNULL(bUpdateNotes,0)) AS bUpdateNotes
FROM CURdUserItems WITH (NOLOCK)
WHERE UserId=@uid AND ItemId=@itemId", conn))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@itemId", itemId);
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    userRowCount = rd["Cnt"] == DBNull.Value ? 0 : Convert.ToInt32(rd["Cnt"]);
                    userUpdateMoney = rd["bUpdateMoney"] == DBNull.Value ? 0 : Convert.ToInt32(rd["bUpdateMoney"]);
                    userUpdateNotes = rd["bUpdateNotes"] == DBNull.Value ? 0 : Convert.ToInt32(rd["bUpdateNotes"]);
                }
            }

            var useUser = userRowCount > 0;
            return Ok(new
            {
                userId,
                itemId,
                source = useUser ? "user" : "group",
                bUpdateMoney = useUser ? userUpdateMoney : grpUpdateMoney,
                bUpdateNotes = useUser ? userUpdateNotes : grpUpdateNotes
            });
        }
    }
}
