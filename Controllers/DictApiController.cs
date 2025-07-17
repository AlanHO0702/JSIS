using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; // 這個要using
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PcbErpApi.Pages.EMOdProdInfo.IndexModel;

[Route("api/[controller]/[action]")]
[ApiController]
public class DictApiController : ControllerBase
{
    private readonly string _connStr;

    public DictApiController(PcbErpContext context, IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDictFields([FromBody] List<UpdateDictFieldInput> list)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();

            foreach (var input in list)
            {
                var cmd = new SqlCommand(@"
                    UPDATE CURdTableField 
                    SET DisplayLabel = @DisplayLabel, 
                        DataType = @DataType, 
                        FieldNote = @FieldNote, 
                        SerialNum = @SerialNum, 
                        Visible = @Visible
                    WHERE TableName = @TableName AND FieldName = @FieldName", conn);

                cmd.Parameters.AddWithValue("@TableName", input.TableName ?? "");
                cmd.Parameters.AddWithValue("@DisplayLabel", (object?)input.DisplayLabel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DataType", (object?)input.DataType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FieldNote", (object?)input.FieldNote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SerialNum", (object?)input.SerialNum ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Visible", (object?)input.Visible ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FieldName", (object)input.FieldName);

                await cmd.ExecuteNonQueryAsync();
                
            }
        }
        return Ok(new { success = true });
    }
}
