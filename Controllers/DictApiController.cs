using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; 
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
                var cmd = new SqlCommand();
                var sql = "UPDATE CURdTableField SET ";
                var setList = new List<string>();

                cmd.Parameters.AddWithValue("@TableName", input.TableName ?? "");
                cmd.Parameters.AddWithValue("@FieldName", input.FieldName ?? "");

                if (input.DisplayLabel != null)
                {
                    setList.Add("DisplayLabel = @DisplayLabel");
                    cmd.Parameters.AddWithValue("@DisplayLabel", input.DisplayLabel);
                }
                if (input.DataType != null)
                {
                    setList.Add("DataType = @DataType");
                    cmd.Parameters.AddWithValue("@DataType", input.DataType);
                }
                if (input.FieldNote != null)
                {
                    setList.Add("FieldNote = @FieldNote");
                    cmd.Parameters.AddWithValue("@FieldNote", input.FieldNote);
                }
                if (input.SerialNum != null)
                {
                    setList.Add("SerialNum = @SerialNum");
                    cmd.Parameters.AddWithValue("@SerialNum", input.SerialNum);
                }
                if (input.Visible != null)
                {
                    setList.Add("Visible = @Visible");
                    cmd.Parameters.AddWithValue("@Visible", input.Visible);
                }
                if (input.LookupResultField != null)
                {
                    setList.Add("LookupResultField = @LookupResultField");
                    cmd.Parameters.AddWithValue("@LookupResultField", input.LookupResultField);
                }

                if (setList.Count == 0) continue; // 沒有欄位要改就 skip

                sql += string.Join(", ", setList);
                sql += " WHERE TableName = @TableName AND FieldName = @FieldName";
                cmd.CommandText = sql;
                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync();
            }
        }
        return Ok(new { success = true });
    }
    public class DictLayoutInput
    {
        public string FieldName { get; set; }
        public int? iShowWhere { get; set; }
        public int? iLayRow { get; set; }
        public int? iLayColumn { get; set; }
        public int? iFieldWidth { get; set; }
        public int? iFieldHeight { get; set; }
        public int? iFieldTop { get; set; }
        public int? iFieldLeft { get; set; }
}

[HttpPost]
public async Task<IActionResult> UpdateDictFieldsLayout([FromBody] List<DictLayoutInput> list)
{
    using (var conn = new SqlConnection(_connStr))
    {
        await conn.OpenAsync();
        foreach (var input in list)
        {
            var cmd = new SqlCommand(@"
                UPDATE CURdTableField 
                SET iShowWhere = @iShowWhere, iLayRow = @iLayRow, iLayColumn = @iLayColumn, 
                    iFieldWidth = @iFieldWidth, iFieldHeight = @iFieldHeight,
                    iFieldTop = @iFieldTop, iFieldLeft = @iFieldLeft
                WHERE FieldName = @FieldName", conn);
            cmd.Parameters.AddWithValue("@iShowWhere", (object)input.iShowWhere ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@iLayRow", (object)input.iLayRow ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@iLayColumn", (object)input.iLayColumn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@iFieldWidth", (object)input.iFieldWidth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@iFieldHeight", (object)input.iFieldHeight ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@iFieldTop", (object)input.iFieldTop ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@iFieldLeft", (object)input.iFieldLeft ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FieldName", input.FieldName ?? "");
            await cmd.ExecuteNonQueryAsync();
        }
    }
    return Ok();
}

}
