using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class MessagesModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public MessagesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DataTable MessageList { get; set; } = new DataTable();
        public List<CURdTableField> TableFields { get; set; } = new List<CURdTableField>();
        public List<CURdTableField> KindFields { get; set; } = new List<CURdTableField>();
        public int TotalCount { get; set; }
        public string CurrentUserId { get; set; } = "";

        private static CURdTableField ReadTableField(IDataRecord reader)
        {
            return new CURdTableField
            {
                TableName = reader["TableName"]?.ToString() ?? "",
                FieldName = reader["FieldName"]?.ToString() ?? "",
                DisplayLabel = reader["DisplayLabel"]?.ToString(),
                DisplaySize = reader["DisplaySize"] as int?,
                DataType = reader["DataType"]?.ToString(),
                FormatStr = reader["FormatStr"]?.ToString(),
                Visible = reader["Visible"] as int?,
                ReadOnly = reader["ReadOnly"] as int?,
                SerialNum = reader["SerialNum"] as int?,
                ComboStyle = reader["ComboStyle"] as int?,
                Items = reader["Items"]?.ToString(),
                LookupTable = reader["LookupTable"]?.ToString(),
                LookupKeyField = reader["LookupKeyField"]?.ToString(),
                LookupResultField = reader["LookupResultField"]?.ToString()
            };
        }

        private static async Task<List<CURdTableField>> LoadTableFieldsAsync(SqlConnection connection, string tableName)
        {
            var list = new List<CURdTableField>();
            var dictQuery = @"
                SELECT *
                FROM CURdTableField
                WHERE TableName = @TableName
                ORDER BY SerialNum";

            using (var command = new SqlCommand(dictQuery, connection))
            {
                command.Parameters.AddWithValue("@TableName", tableName);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(ReadTableField(reader));
                    }
                }
            }

            return list;
        }

        public async Task OnGetAsync(string? userId = null, string? useId = null)
        {
            userId = HttpContext.Items["UserId"]?.ToString()
                     ?? userId
                     ?? User.Identity?.Name
                     ?? "admin";
            CurrentUserId = userId;

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                TableFields = await LoadTableFieldsAsync(connection, "CURdMsg");

                // 查詢 CURdMsg 並 JOIN 取得 Lookup 欄位（根據 Delphi 定義）
                var sql = @"
                    SELECT
                        m.*,
                        ISNULL(p.PaperName, '') AS Lk_ToPaperName,
                        ISNULL(u.UserName, '') AS Lk_FromUserName,
                        ISNULL(k.KindName, '') AS Lk_KindName
                    FROM CURdMsg m WITH (NOLOCK)
                    LEFT JOIN CURdPaperInfo p WITH (NOLOCK) ON m.ToPaperId = p.PaperId
                    LEFT JOIN CURdUsers u WITH (NOLOCK) ON m.FromUserId = u.UserId
                    LEFT JOIN CURdMsgKind k WITH (NOLOCK) ON m.Kind = k.Kind
                    WHERE m.ToUserId = @UserId
                    ORDER BY m.BuildDate DESC";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(MessageList);
                    }
                }

                TotalCount = MessageList.Rows.Count;

                // 如果沒有欄位定義,則從 DataTable 自動生成
                if (TableFields.Count == 0 && MessageList.Columns.Count > 0)
                {
                    foreach (DataColumn col in MessageList.Columns)
                    {
                        TableFields.Add(new CURdTableField
                        {
                            TableName = "CURdMsg",
                            FieldName = col.ColumnName,
                            DisplayLabel = col.ColumnName,
                            Visible = 1
                        });
                    }
                }
            }
        }
    }
}
