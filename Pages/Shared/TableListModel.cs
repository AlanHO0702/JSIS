// /Pages/Shared/TableListModel.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using System.Net.Http.Json;
using System.Reflection;

public abstract class TableListModel<T> : PageModel where T : class, new()
{
    protected readonly HttpClient _httpClient;
    protected readonly ITableDictionaryService _dictService;

    public TableListModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _dictService = dictService;
    }

    public List<T> Items { get; set; } = new();
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((TotalCount) / (double)PageSize);
    public List<CURdTableField> FieldDictList { get; set; }
    public List<TableFieldViewModel> TableFields { get; set; } = new();

    public abstract string TableName { get; }
    public virtual string ApiPagedUrl => $"/api/{TableName}/paged";

    public virtual async Task OnGetAsync([FromQuery(Name = "page")] int? page)
    {
        PageNumber = page ?? 1;
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var apiUrl = $"{baseUrl}{ApiPagedUrl}?page={PageNumber}&pageSize={PageSize}";
        var resp = await _httpClient.GetFromJsonAsync<ApiResult>(apiUrl);

        Items = resp?.data ?? new List<T>();
        TotalCount = resp?.totalCount ?? 0;

        FieldDictList = _dictService.GetFieldDict(TableName, typeof(T));
        TableFields = FieldDictList
            .Where(x => x.Visible == 1)
            .OrderBy(x => x.SerialNum)
            .Select(x => new TableFieldViewModel
            {
                FieldName = x.FieldName,
                DisplayLabel = x.DisplayLabel,
                SerialNum = x.SerialNum ?? 0,
                Visible = x.Visible == 1
            }).ToList();
    }

    public class ApiResult
    {
        public int totalCount { get; set; }
        public List<T>? data { get; set; }
    }

    



}
