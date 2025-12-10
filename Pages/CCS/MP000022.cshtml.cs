using System.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcbErpApi.Data;

namespace PcbErpApi.Pages.CCS
{
    public class MP000022Model : PageModel
    {
        private readonly string _connStr;
        private readonly ILogger<MP000022Model> _logger;

        public MP000022Model(IConfiguration cfg, PcbErpContext db, ILogger<MP000022Model> logger)
        {
            _connStr = cfg.GetConnectionString("Default")
                       ?? db?.Database.GetDbConnection().ConnectionString
                       ?? throw new InvalidOperationException("缺少資料庫連線字串");
            _logger = logger;
        }

        public string ItemId => "MP000022";
        public string PageTitle => "廠商物料類別設定";
        public List<LookupItem> Suppliers { get; private set; } = new();
        public List<LookupItem> MatClasses { get; private set; } = new();
        public string? LoadError { get; private set; }

        public async Task OnGetAsync()
        {
            ViewData["Title"] = $"{ItemId} {PageTitle}";
            try
            {
                Suppliers = await LoadSuppliersAsync();
                MatClasses = await LoadMatClassesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load MP000022 initial data failed");
                LoadError = ex.Message;
            }
        }

        private async Task<List<LookupItem>> LoadSuppliersAsync()
        {
            const string sql = @"
SELECT CompanyId AS Id,
       ISNULL(NULLIF(ShortName, ''), CompanyName) AS Name
  FROM dbo.AJNdMTLSupplier WITH (NOLOCK)
 ORDER BY CompanyId";

            return await QueryListAsync(sql, r =>
            {
                var id = r["Id"]?.ToString() ?? string.Empty;
                var name = r["Name"]?.ToString() ?? string.Empty;
                return new LookupItem(id, string.IsNullOrWhiteSpace(name) ? id : name);
            });
        }

        private async Task<List<LookupItem>> LoadMatClassesAsync()
        {
            const string sql = @"
SELECT MatClass AS Id,
       ClassName AS Name
  FROM dbo.MINdMatClass WITH (NOLOCK)
 ORDER BY MatClass";

            return await QueryListAsync(sql, r =>
            {
                var id = r["Id"]?.ToString() ?? string.Empty;
                var name = r["Name"]?.ToString() ?? string.Empty;
                return new LookupItem(id, string.IsNullOrWhiteSpace(name) ? id : name);
            });
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
