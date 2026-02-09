using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using PcbErpApi.Models;
using System.Text.Json.Serialization;
using System.Linq;

namespace PcbErpApi.Pages
{
    public class NoticeBoardListModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public NoticeBoardListModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DataTable Rows { get; set; } = new DataTable();
        public List<CURdTableField> TableFields { get; set; } = new List<CURdTableField>();
        public string CurrentUserId { get; set; } = "";

        private sealed class NoticeMiniItem
        {
            public int SerialNum { get; set; }
            public string Subject { get; set; } = "";
            public string Text { get; set; } = "";
            public string ShortDate { get; set; } = "";
            public string DateRange { get; set; } = "";
            public string BuildDate { get; set; } = "";
            public string UserName { get; set; } = "";

            [JsonIgnore]
            public DateTime? BuildDateValue { get; set; }
        }

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

        private static async Task<Dictionary<string, string>> LoadUserDictAsync(SqlConnection connection)
        {
            var usersDict = new Dictionary<string, string>();
            var userQuery = "SELECT UserId, UserName FROM CURdUsers WITH (NOLOCK)";
            using (var userCommand = new SqlCommand(userQuery, connection))
            using (var reader = await userCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var uid = reader["UserId"]?.ToString() ?? "";
                    var uname = reader["UserName"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(uid))
                    {
                        usersDict[uid] = uname;
                    }
                }
            }

            return usersDict;
        }

        public async Task OnGetAsync(string? userId = null)
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

                // 載入欄位定義
                TableFields = await LoadTableFieldsAsync(connection, "CURdNoticeBoardInq");

                // 執行 stored procedure 取得佈告欄資料
                using (var command = new SqlCommand("CURdGetNoticeBoardInq", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(Rows);
                    }
                }

                // 如果結果中沒有 UserName 欄位,從 CURdUsers 表中 join UserName
                if (Rows.Rows.Count > 0 && !Rows.Columns.Contains("UserName") && !Rows.Columns.Contains("Lk_UserName"))
                {
                    // 新增 UserName 欄位
                    Rows.Columns.Add("UserName", typeof(string));

                    // 取得所有 users
                    var usersDict = new Dictionary<string, string>();
                    var userQuery = "SELECT UserId, UserName FROM CURdUsers WITH (NOLOCK)";
                    using (var userCommand = new SqlCommand(userQuery, connection))
                    using (var reader = await userCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var uid = reader["UserId"]?.ToString() ?? "";
                            var uname = reader["UserName"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(uid))
                            {
                                usersDict[uid] = uname;
                            }
                        }
                    }

                    // 填入 UserName
                    foreach (DataRow row in Rows.Rows)
                    {
                        var postUserId = row["PostUserId"]?.ToString() ?? "";
                        if (usersDict.TryGetValue(postUserId, out var userName))
                        {
                            row["UserName"] = userName;
                        }
                        else
                        {
                            row["UserName"] = postUserId;
                        }
                    }
                }

                // 如果沒有欄位定義,使用預設欄位
                if (TableFields.Count == 0 && Rows.Columns.Count > 0)
                {
                    foreach (DataColumn col in Rows.Columns)
                    {
                        TableFields.Add(new CURdTableField
                        {
                            TableName = "CURdNoticeBoardInq",
                            FieldName = col.ColumnName,
                            DisplayLabel = col.ColumnName,
                            Visible = 1
                        });
                    }
                }
            }
        }

        // 標記為已讀的 API
        public async Task<IActionResult> OnGetMiniListAsync(string? userId = null, int take = 4)
        {
            userId = HttpContext.Items["UserId"]?.ToString()
                     ?? userId
                     ?? User.Identity?.Name
                     ?? "admin";

            take = Math.Clamp(take, 1, 8);

            var items = new List<NoticeMiniItem>();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var rows = new DataTable();
                using (var command = new SqlCommand("CURdGetNoticeBoardInq", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(rows);
                    }
                }

                Dictionary<string, string>? usersDict = null;
                var hasUserName = rows.Columns.Contains("Lk_UserName") || rows.Columns.Contains("UserName");
                if (!hasUserName)
                {
                    usersDict = await LoadUserDictAsync(connection);
                }

                foreach (DataRow row in rows.Rows)
                {
                    var serialNumStr = row["SerialNum"]?.ToString() ?? "";
                    _ = int.TryParse(serialNumStr, out var serialNum);

                    var postUserId = row["PostUserId"]?.ToString() ?? "";
                    var userName = postUserId;
                    if (rows.Columns.Contains("Lk_UserName"))
                    {
                        userName = row["Lk_UserName"]?.ToString() ?? postUserId;
                    }
                    else if (rows.Columns.Contains("UserName"))
                    {
                        userName = row["UserName"]?.ToString() ?? postUserId;
                    }
                    else if (usersDict != null && usersDict.TryGetValue(postUserId, out var lookupName))
                    {
                        userName = lookupName;
                    }

                    var buildDateValue = row["BuildDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["BuildDate"])
                        : (DateTime?)null;

                    var beginDate = row["BeginDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["BeginDate"])
                        : (DateTime?)null;
                    var endDate = row["EndDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["EndDate"])
                        : (DateTime?)null;

                    var subject = row["Subjects"]?.ToString() ?? "";
                    var text = row["BoardText"]?.ToString() ?? "";

                    var shortDate = buildDateValue?.ToString("MM/dd")
                                    ?? beginDate?.ToString("MM/dd")
                                    ?? "";
                    var buildDate = buildDateValue?.ToString("yyyy/MM/dd") ?? "";
                    var dateRange = beginDate.HasValue || endDate.HasValue
                        ? $"{beginDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}"
                        : "";

                    items.Add(new NoticeMiniItem
                    {
                        SerialNum = serialNum,
                        Subject = subject,
                        Text = text,
                        ShortDate = shortDate,
                        DateRange = dateRange,
                        BuildDate = buildDate,
                        UserName = userName,
                        BuildDateValue = buildDateValue
                    });
                }
            }

            var resultItems = items
                .OrderByDescending(i => i.BuildDateValue ?? DateTime.MinValue)
                .Take(take)
                .ToList();

            return new JsonResult(new { success = true, items = resultItems });
        }

        public async Task<IActionResult> OnGetMiniListAllAsync(int take = 6)
        {
            take = Math.Clamp(take, 1, 12);

            var items = new List<NoticeMiniItem>();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var usersDict = await LoadUserDictAsync(connection);

                var query = @"
                    SELECT TOP (@Take)
                        SerialNum, PostUserId, Subjects, BoardText, BeginDate, EndDate, BuildDate
                    FROM CURdNoticeBoard WITH (NOLOCK)
                    ORDER BY ISNULL(BuildDate, BeginDate) DESC, SerialNum DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Take", take);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var serialNumStr = reader["SerialNum"]?.ToString() ?? "";
                            _ = int.TryParse(serialNumStr, out var serialNum);

                            var postUserId = reader["PostUserId"]?.ToString() ?? "";
                            var userName = usersDict.TryGetValue(postUserId, out var lookupName)
                                ? lookupName
                                : postUserId;

                            var buildDateValue = reader["BuildDate"] != DBNull.Value
                                ? Convert.ToDateTime(reader["BuildDate"])
                                : (DateTime?)null;
                            var beginDate = reader["BeginDate"] != DBNull.Value
                                ? Convert.ToDateTime(reader["BeginDate"])
                                : (DateTime?)null;
                            var endDate = reader["EndDate"] != DBNull.Value
                                ? Convert.ToDateTime(reader["EndDate"])
                                : (DateTime?)null;

                            var subject = reader["Subjects"]?.ToString() ?? "";
                            var text = reader["BoardText"]?.ToString() ?? "";

                            var shortDate = buildDateValue?.ToString("MM/dd")
                                            ?? beginDate?.ToString("MM/dd")
                                            ?? "";
                            var buildDate = buildDateValue?.ToString("yyyy/MM/dd") ?? "";
                            var dateRange = beginDate.HasValue || endDate.HasValue
                                ? $"{beginDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}"
                                : "";

                            items.Add(new NoticeMiniItem
                            {
                                SerialNum = serialNum,
                                Subject = subject,
                                Text = text,
                                ShortDate = shortDate,
                                DateRange = dateRange,
                                BuildDate = buildDate,
                                UserName = userName,
                                BuildDateValue = buildDateValue
                            });
                        }
                    }
                }
            }

            return new JsonResult(new { success = true, items });
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int serialNum, string userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    IF NOT EXISTS (SELECT 1 FROM CURdNoticeBoardRead WHERE NoticeId = @NoticeId AND UserId = @UserId)
                    BEGIN
                        INSERT INTO CURdNoticeBoardRead (NoticeId, UserId, ReadTime)
                        VALUES (@NoticeId, @UserId, GETDATE())
                    END";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NoticeId", serialNum);
                    command.Parameters.AddWithValue("@UserId", userId);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return new JsonResult(new { success = true });
        }
    }
}
