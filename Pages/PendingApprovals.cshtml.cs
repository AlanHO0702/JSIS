using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class PendingApprovalsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public PendingApprovalsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DataTable PendingList { get; set; } = new DataTable();
        public List<CURdTableField> TableFields { get; set; } = new List<CURdTableField>();
        public int TotalCount { get; set; }
        public string CurrentUserId { get; set; } = "";

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
            string? sort3 = null)
        {
            // 從 HttpContext.Items 取得 UserId（由 Middleware 設定）
            // 如果沒有，從參數取得，再沒有就使用預設值
            userId = HttpContext.Items["UserId"]?.ToString()
                     ?? userId
                     ?? User.Identity?.Name
                     ?? "admin";
            CurrentUserId = userId;

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // 查詢資料辭典取得完整欄位資訊
                var dictQuery = @"
                    SELECT *
                    FROM CURdTableField
                    WHERE TableName = 'XFLdeFlowInfo'
                    ORDER BY SerialNum";

                using (var command = new SqlCommand(dictQuery, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var field = new CURdTableField
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
                            TableFields.Add(field);
                        }
                    }
                }

                // 建立查詢條件
                var whereConditions = new List<string> { "f.USERID = @UserId" };
                var parameters = new List<SqlParameter> { new SqlParameter("@UserId", userId) };

                if (!string.IsNullOrWhiteSpace(mailSeq))
                {
                    whereConditions.Add("f.MAILSEQ = @MailSeq");
                    parameters.Add(new SqlParameter("@MailSeq", mailSeq));
                }

                if (!string.IsNullOrWhiteSpace(subject))
                {
                    whereConditions.Add("f.SUBJECT LIKE @Subject");
                    parameters.Add(new SqlParameter("@Subject", $"%{subject}%"));
                }

                if (!string.IsNullOrWhiteSpace(applicant))
                {
                    whereConditions.Add("f.APPLICANT = @Applicant");
                    parameters.Add(new SqlParameter("@Applicant", applicant));
                }

                if (!string.IsNullOrWhiteSpace(sender))
                {
                    whereConditions.Add("f.SENDER = @Sender");
                    parameters.Add(new SqlParameter("@Sender", sender));
                }

                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    whereConditions.Add("f.CDATE >= @StartDate");
                    parameters.Add(new SqlParameter("@StartDate", startDate));
                }

                if (!string.IsNullOrWhiteSpace(endDate))
                {
                    whereConditions.Add("f.CDATE < DATEADD(day, 1, @EndDate)");
                    parameters.Add(new SqlParameter("@EndDate", endDate));
                }

                if (!string.IsNullOrWhiteSpace(paperId))
                {
                    whereConditions.Add("f.PAPERID = @PaperId");
                    parameters.Add(new SqlParameter("@PaperId", paperId));
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    whereConditions.Add("f.STATUSNAME = @Status");
                    parameters.Add(new SqlParameter("@Status", status));
                }

                // 建立排序條件
                var orderByClause = "f.MAILSEQ DESC";
                var sortFields = new List<string>();

                if (!string.IsNullOrWhiteSpace(sort1)) sortFields.Add($"f.{sort1}");
                if (!string.IsNullOrWhiteSpace(sort2)) sortFields.Add($"f.{sort2}");
                if (!string.IsNullOrWhiteSpace(sort3)) sortFields.Add($"f.{sort3}");

                if (sortFields.Count > 0)
                {
                    orderByClause = string.Join(", ", sortFields);
                }

                // 直接從 XFLdeFlowInfo 查詢簽核待審列表
                // 加入 XFLdPRCINST 來取得 PRCSEQ (流程序號)
                var query = $@"
                    SELECT f.*,
                           ISNULL(s.ItemId, f.PAPERID) as ItemId,
                           p.SEQ as PRCSEQ
                    FROM XFLdeFlowInfo f
                    LEFT JOIN CURdOCXTableSetUp s ON f.PAPERID = s.TableName AND s.TableKind LIKE '%Master%'
                    LEFT JOIN XFLdPRCINST p ON f.PAPERID = p.PAPERID
                                            AND f.PAPERNUM = p.PAPERNUM
                                            AND f.USEID = p.USEID
                                            AND f.SYSTEMID = p.SYSTEMID
                    WHERE {string.Join(" AND ", whereConditions)}
                    ORDER BY {orderByClause}";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(PendingList);
                    }
                }

                // 取得待審總數
                TotalCount = PendingList.Rows.Count;
            }
        }
    }
}
