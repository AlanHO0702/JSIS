using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class WorkingListModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public WorkingListModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DataTable Rows { get; set; } = new DataTable();
        public List<CURdTableField> TableFields { get; set; } = new List<CURdTableField>();
        public string CurrentUserId { get; set; } = "";
        public string CurrentUseId { get; set; } = "";

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
                Visible = reader["Visible"] as int?
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

            useId = HttpContext.Items["UseId"]?.ToString()
                    ?? useId
                    ?? "";
            CurrentUseId = useId;

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                TableFields = await LoadTableFieldsAsync(connection, "CURdOCXWorkingList");

                using (var command = new SqlCommand("CURdOCXWorkingList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@UseId", useId ?? "");
                    command.Parameters.AddWithValue("@forCount", 0);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(Rows);
                    }
                }

                if (TableFields.Count == 0 && Rows.Columns.Count > 0)
                {
                    foreach (DataColumn col in Rows.Columns)
                    {
                        TableFields.Add(new CURdTableField
                        {
                            TableName = "CURdOCXWorkingList",
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
