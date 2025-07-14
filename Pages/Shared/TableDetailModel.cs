// /Pages/Shared/TableDetailModel.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Models;
using System.Net.Http.Json;
using System.Reflection;

public abstract class TableDetailModel<T> : PageModel where T : class, new()
{
    protected readonly HttpClient _httpClient;
    protected readonly ITableDictionaryService _dictService;

    public TableDetailModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _dictService = dictService;
    }

    public List<T> Items { get; set; } = new();
    public List<CURdTableField> FieldDictList { get; set; }
    public List<TableFieldViewModel> TableFields { get; set; } = new();

    public abstract string ApiDetailUrl { get; }
    public virtual Dictionary<string, string>? ApiQueryParameters => null;
    public string MasterKey { get; set; }
    public abstract string TableName { get; }

    // 這裡只要一個 FetchDataAsync，不要再寫方法宣告
    protected async Task FetchDataAsync(string masterKey)
    {
        MasterKey = masterKey;

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var url = $"{baseUrl}{ApiDetailUrl}";

        if (ApiQueryParameters != null)
        {
            var query = string.Join("&", ApiQueryParameters.Select(p => $"{p.Key}={p.Value}"));
            url = $"{url}?{query}";
        }
        else if (!string.IsNullOrEmpty(masterKey))
        {
            url = $"{url}?{GetDefaultMasterKeyName()}={masterKey}";
        }

        Items = await _httpClient.GetFromJsonAsync<List<T>>(url) ?? new();

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

    public virtual string GetDefaultMasterKeyName() => "PaperNum";
}

