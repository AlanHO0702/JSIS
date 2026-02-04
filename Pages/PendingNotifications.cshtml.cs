using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class PendingNotificationsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public PendingNotificationsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DataTable PendingList { get; set; } = new DataTable();
        public List<CURdTableField> TableFields { get; set; } = new List<CURdTableField>();
        public List<CURdTableField> FlowHistoryFields { get; set; } = new List<CURdTableField>();
        public List<CURdTableField> FlowCommentFields { get; set; } = new List<CURdTableField>();
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

        public async Task OnGetAsync(
            string? userId = null,
            string? mailSeq = null,
            string? subject = null,
            string? applicant = null,
            string? sender = null,
            string? startDate = null,
            string? endDate = null,
            string? paperId = null,
            string? status = null,
            string? sort1 = null,
            string? sort2 = null,
            string? sort3 = null,
            string? paperNum = null)
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

                TableFields = await LoadTableFieldsAsync(connection, "XFLdWORKLIST");
                FlowHistoryFields = await LoadTableFieldsAsync(connection, "CURdFlowAlongHis");
                FlowCommentFields = await LoadTableFieldsAsync(connection, "CURdFlowAlongComt");

                var where = "";
                if (!string.IsNullOrWhiteSpace(mailSeq))
                {
                    var escapedMailSeq = mailSeq.Replace("'", "''");
                    where += $" AND t1.SEQ = '{escapedMailSeq}'";
                }
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    var escapedSubject = subject.Replace("'", "''");
                    where += $" AND t1.SUBJECT LIKE N'%{escapedSubject}%'";
                }
                if (!string.IsNullOrWhiteSpace(applicant))
                {
                    var escapedApplicant = applicant.Replace("'", "''");
                    where += $" AND t7.APPLICANT = '{escapedApplicant}'";
                }
                if (!string.IsNullOrWhiteSpace(sender))
                {
                    var escapedSender = sender.Replace("'", "''");
                    where += $" AND t1.SENDER = '{escapedSender}'";
                }
                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    var escapedStartDate = startDate.Replace("'", "''");
                    where += $" AND t7.CDATE >= '{escapedStartDate}'";
                }
                if (!string.IsNullOrWhiteSpace(endDate))
                {
                    var escapedEndDate = endDate.Replace("'", "''");
                    where += $" AND t7.CDATE < dateadd(day,1,'{escapedEndDate}')";
                }
                if (!string.IsNullOrWhiteSpace(paperId))
                {
                    var escapedPaperId = paperId.Replace("'", "''");
                    where += $" AND t7.PAPERID = '{escapedPaperId}'";
                }

                var hasFilter = !string.IsNullOrWhiteSpace(where) || !string.IsNullOrWhiteSpace(status);
                var useWhere = hasFilter ? 1 : 0;
                var statusValue = hasFilter ? (status ?? "") : "0";
                var usePaperNum = !string.IsNullOrWhiteSpace(paperNum) ? 1 : 0;

                var sql = @"
                    EXEC XFLdWORKLIST
                        @UserId,
                        @Type,
                        @Flag,
                        @Sort1,
                        @Sort2,
                        @Sort3,
                        @UseWhere,
                        @Where,
                        @Status,
                        @UsePaperNum,
                        @PaperNum";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Type", 2);
                    command.Parameters.AddWithValue("@Flag", 0);
                    command.Parameters.AddWithValue("@Sort1", sort1 ?? "");
                    command.Parameters.AddWithValue("@Sort2", sort2 ?? "");
                    command.Parameters.AddWithValue("@Sort3", sort3 ?? "");
                    command.Parameters.AddWithValue("@UseWhere", useWhere);
                    command.Parameters.AddWithValue("@Where", where);
                    command.Parameters.AddWithValue("@Status", statusValue);
                    command.Parameters.AddWithValue("@UsePaperNum", usePaperNum);
                    command.Parameters.AddWithValue("@PaperNum", paperNum ?? "");

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(PendingList);
                    }
                }

                TotalCount = PendingList.Rows.Count;

                if (TableFields.Count == 0 && PendingList.Columns.Count > 0)
                {
                    foreach (DataColumn col in PendingList.Columns)
                    {
                        TableFields.Add(new CURdTableField
                        {
                            TableName = "XFLdWORKLIST",
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
