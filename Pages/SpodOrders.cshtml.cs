using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;

public class SpodOrdersModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly ITableDictionaryService _dictService;
    public SpodOrdersModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _dictService = dictService;
    }

    public List<SpodOrderMain> Orders { get; set; } = new();
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((TotalCount) / (double)PageSize);
    public List<CURdTableField> FieldDictList { get; set; }
    public List<TableFieldViewModel> TableFields { get; set; } = new();
    public async Task OnGetAsync([FromQuery(Name = "page")] int? page)
    {
        PageNumber = page ?? 1;
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var apiUrl = $"{baseUrl}/api/SPOdOrderMains/paged?page={PageNumber}&pageSize={PageSize}";
        var resp = await _httpClient.GetFromJsonAsync<ApiResult>(apiUrl);
        Orders = resp?.data ?? new List<SpodOrderMain>();
        TotalCount = resp?.totalCount ?? 0;

 
        FieldDictList = _dictService.GetFieldDict("SpodOrderMain", typeof(SpodOrderMain));
        TableFields = FieldDictList
        .Where(x => x.Visible == 1) // 只取可見欄位
        .OrderBy(x => x.SerialNum)
        .Select(x => new TableFieldViewModel
        {
            FieldName = x.FieldName,
            DisplayLabel = x.DisplayLabel,
            SerialNum = x.SerialNum ?? 0,
            Visible = x.Visible == 1
        }).ToList();
    }

    // API Response 物件
    public class ApiResult
    {
        public int totalCount { get; set; }
        public List<SpodOrderMain>? data { get; set; }
    }

    public string GetStatusName(int status)
    {
        return status switch
        {
            0 => "作業中",
            1 => "已確認",
            2 => "已作廢",
            3 => "審核中",
            4 => "已結案",
            _ => "未知"
        };
    }

    public string GetStatusColor(int status)
    {
        return status switch
        {
            1 => "success",
            2 => "danger",
            4 => "primary",
            _ => "light"
        };
    }

    public class TableFieldViewModel
    {
        public string FieldName { get; set; }
        public string DisplayLabel { get; set; }
        public int SerialNum { get; set; }
        public bool Visible { get; set; }
    }

}
