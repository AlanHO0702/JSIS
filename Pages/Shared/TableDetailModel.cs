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
    public Dictionary<string, object> HeaderData { get; set; }
    public List<TableFieldViewModel> HeaderTableFields { get; set; }
    public List<(int TabNum, string TabTitle)> FieldTabs { get; set; } = new();


    public abstract string ApiDetailUrl { get; }
    public virtual Dictionary<string, string>? ApiQueryParameters => null;
    public string MasterKey { get; set; }
    public abstract string TableName { get; }

    // ⚠️ 這裡才是 protected method，寫在類別最外層
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

        FieldTabs = FieldDictList
        .Where(x => x.Visible == 1 && (x.iShowWhere ?? 0) >= 0)
        .Select(x => x.iShowWhere ?? 0)
        .Distinct()
        .OrderBy(x => x)
        .Select(i => (i, i == 0 ? "主頁" : $"分頁{i}"))
        .ToList();

        HeaderTableFields = FieldDictList
        .Where(x => x.Visible == 1)
        .OrderBy(x => x.SerialNum)
        .Select(x => new TableFieldViewModel
        {
            FieldName = x.FieldName,
            DisplayLabel = x.DisplayLabel,
            iFieldWidth = x.iFieldWidth
        }).ToList();
        
        
    }

    public virtual string GetDefaultMasterKeyName() => "PaperNum";
}
