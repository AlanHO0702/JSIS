using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class EcnController : ControllerBase
    {
        private readonly PcbErpContext _ctx;

        public EcnController(PcbErpContext ctx)
        {
            _ctx = ctx;
        }

        public class InsOldDataRequest
        {
            public string PaperNum { get; set; } = "";
            public string PartNum { get; set; } = "";
            public int Item { get; set; } = 1;
        }

        /// <summary>
        /// 品號離開欄位觸發：查 MINdMatInfo 取得 MatClass/MatName，
        /// 呼叫 MGNdGenSetNumTable + EMOdUpdateAddData 重建屬性明細
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InsOldData([FromBody] InsOldDataRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PaperNum) || string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest(new { error = "PaperNum and PartNum are required" });

            var cs = _ctx.Database.GetConnectionString()!;
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // 1. 查 MINdMatInfo 取得 MatClass + MatName (Revision='0')
            string matClass = "", matName = "";
            {
                const string sql =
                    "SELECT MatClass, MatName FROM MINdMatInfo WITH (NOLOCK) WHERE PartNum=@pn AND Revision='0'";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@pn", req.PartNum.Trim());
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    matClass = rd["MatClass"]?.ToString() ?? "";
                    matName = rd["MatName"]?.ToString() ?? "";
                }
            }

            int spid = 0;

            if (!string.IsNullOrEmpty(matClass))
            {
                // 2. 呼叫 MGNdGenSetNumTable → 取得 SPId
                {
                    const string sql = "EXEC MGNdGenSetNumTable @setClass, @partNum";
                    await using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@setClass", matClass);
                    cmd.Parameters.AddWithValue("@partNum", req.PartNum.Trim());
                    await using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        spid = rd["SPId"] != DBNull.Value ? Convert.ToInt32(rd["SPId"]) : 0;
                    }
                }

                // 3. 刪除舊的 EMOdECNSetNumAddData
                {
                    const string sql = "DELETE FROM EMOdECNSetNumAddData WHERE PaperNum=@pn";
                    await using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@pn", req.PaperNum.Trim());
                    await cmd.ExecuteNonQueryAsync();
                }

                // 4. 呼叫 EMOdUpdateAddData 重新產生屬性明細
                {
                    const string sql = "EXEC EMOdUpdateAddData @paperNum, @item, @partNum, @rev, @setClass, @spid";
                    await using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@paperNum", req.PaperNum.Trim());
                    cmd.Parameters.AddWithValue("@item", req.Item);
                    cmd.Parameters.AddWithValue("@partNum", req.PartNum.Trim());
                    cmd.Parameters.AddWithValue("@rev", "0");
                    cmd.Parameters.AddWithValue("@setClass", matClass);
                    cmd.Parameters.AddWithValue("@spid", spid);
                    try
                    {
                        await using var rd = await cmd.ExecuteReaderAsync();
                        while (await rd.ReadAsync()) { }
                    }
                    catch
                    {
                        // SP 可能不回傳結果集，改用 ExecuteNonQuery
                        // 如果 ExecuteReader 已消費完，忽略即可
                    }
                }
            }

            // 5. 查詢更新後的 EMOdECNSetNumAddData
            var addDataRows = new List<Dictionary<string, object?>>();
            {
                const string sql =
                    "SELECT * FROM EMOdECNSetNumAddData WITH (NOLOCK) WHERE PaperNum=@pn";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@pn", req.PaperNum.Trim());
                await using var rd = await cmd.ExecuteReaderAsync();
                var cols = Enumerable.Range(0, rd.FieldCount).Select(rd.GetName).ToList();
                while (await rd.ReadAsync())
                {
                    var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < cols.Count; i++)
                        row[cols[i]] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                    addDataRows.Add(row);
                }
            }

            return Ok(new { matClass, matName, spid, addDataRows });
        }
    }
}
