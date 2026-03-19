using System.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Pages.MIN
{
    public class MT000004Model : PageModel
    {
        private readonly string _connStr;
        private readonly ILogger<MT000004Model> _logger;

        public MT000004Model(IConfiguration cfg, PcbErpContext db, ILogger<MT000004Model> logger)
        {
            _connStr = cfg.GetConnectionString("Default")
                       ?? db?.Database.GetDbConnection().ConnectionString
                       ?? throw new InvalidOperationException("Missing connection string.");
            _logger = logger;
        }

        public string ItemId => "MT000004";
        public string PageTitle => "部門請領權限設定";
        public List<LookupItem> Departments { get; private set; } = new();
        public List<LookupItem> MatClasses { get; private set; } = new();
        public List<LookupItem> Parts { get; private set; } = new();
        public bool HidePartTab { get; private set; }
        public string? LoadError { get; private set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = $"{ItemId} {PageTitle}";
            try
            {
                Departments = await LoadDepartmentsAsync();
                MatClasses = await LoadMatClassesAsync();
                Parts = await LoadPartsAsync();
                HidePartTab = await LoadHidePartTabFlagAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load MT000004 initial data failed");
                LoadError = ex.Message;
            }
        }

        private async Task<List<LookupItem>> LoadDepartmentsAsync()
        {
            const string sql = @"
SELECT DepartId, DepartName
  FROM dbo.AJNdDepart WITH (NOLOCK)
 ORDER BY DepartId";
            return await QueryListAsync(sql, r =>
            {
                var id = (r["DepartId"]?.ToString() ?? string.Empty).Trim();
                var name = (r["DepartName"]?.ToString() ?? string.Empty).Trim();
                return new LookupItem(id, string.IsNullOrWhiteSpace(name) ? id : name);
            });
        }

        private async Task<List<LookupItem>> LoadMatClassesAsync()
        {
            const string sql = @"
SELECT MatClass, ClassName
  FROM dbo.MINdMatClass WITH (NOLOCK)
 ORDER BY MatClass";
            return await QueryListAsync(sql, r =>
            {
                var id = (r["MatClass"]?.ToString() ?? string.Empty).Trim();
                var name = (r["ClassName"]?.ToString() ?? string.Empty).Trim();
                return new LookupItem(id, string.IsNullOrWhiteSpace(name) ? id : name);
            });
        }

        private async Task<List<LookupItem>> LoadPartsAsync()
        {
            const string sql = @"
SELECT PartNum, MatName
  FROM dbo.MINdMatInfo WITH (NOLOCK)
 ORDER BY PartNum";
            return await QueryListAsync(sql, r =>
            {
                var id = (r["PartNum"]?.ToString() ?? string.Empty).Trim();
                var name = (r["MatName"]?.ToString() ?? string.Empty).Trim();
                return new LookupItem(id, string.IsNullOrWhiteSpace(name) ? id : name);
            });
        }

        private async Task<bool> LoadHidePartTabFlagAsync()
        {
            const string sql = @"
SELECT TOP 1 ISNULL([Value], '') AS [Value]
  FROM dbo.CURdSysParams WITH (NOLOCK)
 WHERE SystemId = 'MIN'
   AND ParamId = 'DepSetNoShowTab2'";

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            var value = Convert.ToString(await cmd.ExecuteScalarAsync())?.Trim() ?? string.Empty;
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<List<T>> QueryListAsync<T>(string sql, Func<IDataRecord, T> map)
        {
            var result = new List<T>();
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                result.Add(map(rd));
            }
            return result;
        }

        public record LookupItem(string Id, string Name);
    }
}
