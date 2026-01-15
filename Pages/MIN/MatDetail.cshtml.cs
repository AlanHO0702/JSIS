using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

public class MatDetailModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MatDetailModel> _logger;

    public MatDetailModel(IHttpClientFactory httpClientFactory, ILogger<MatDetailModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? PartNum { get; set; }

    // UseId 固定為 A001 (公司別)
    private const string UseId = "A001";

    public Dictionary<string, object>? BaseData { get; set; }
    public List<Dictionary<string, object>>? StockSummary { get; set; }
    public List<Dictionary<string, object>>? IncomingSupply { get; set; }
    public List<Dictionary<string, object>>? OutgoingDemand { get; set; }
    public List<Dictionary<string, object>>? PreCount { get; set; }

    public string? ErrorMessage { get; set; }
    public int? SPId { get; set; }
    public Dictionary<string, string>? DictTitles { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

            // 先載入辭典標題
            var dictResponse = await client.GetAsync("/api/MatDetailApi/GetDictTitles");
            if (dictResponse.IsSuccessStatusCode)
            {
                DictTitles = await dictResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            }

            if (string.IsNullOrWhiteSpace(PartNum))
            {
                // 如果沒有提供 PartNum，只顯示搜尋介面
                return Page();
            }

            // 呼叫整合 API 取得所有資料
            var response = await client.GetAsync($"/api/MatDetailApi/GetFullDetail?partNum={Uri.EscapeDataString(PartNum)}&useId={UseId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.TryGetProperty("baseData", out var baseDataElem))
                {
                    BaseData = ParseJsonElement(baseDataElem);

                    // 取得 SPId
                    if (BaseData != null && BaseData.ContainsKey("SPId"))
                    {
                        var spIdValue = BaseData["SPId"];
                        if (spIdValue is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            SPId = je.GetInt32();
                        }
                        else if (int.TryParse(spIdValue?.ToString(), out var spIdInt))
                        {
                            SPId = spIdInt;
                        }
                    }
                }

                if (root.TryGetProperty("stockSummary", out var stockElem))
                {
                    StockSummary = ParseJsonArray(stockElem);
                }

                if (root.TryGetProperty("incomingSupply", out var inElem))
                {
                    IncomingSupply = ParseJsonArray(inElem);
                }

                if (root.TryGetProperty("outgoingDemand", out var outElem))
                {
                    OutgoingDemand = ParseJsonArray(outElem);
                }

                if (root.TryGetProperty("preCount", out var preElem))
                {
                    PreCount = ParseJsonArray(preElem);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"API 錯誤 ({response.StatusCode}): {errorContent}";
                _logger.LogError("API Error: {StatusCode}, {Content}", response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"發生錯誤: {ex.Message}";
            _logger.LogError(ex, "Error loading material detail for PartNum: {PartNum}", PartNum);
        }

        return Page();
    }

    // 取得辭典標題，如果沒有則返回預設值
    public string GetDictTitle(string spName, string defaultTitle)
    {
        if (DictTitles != null && DictTitles.TryGetValue(spName, out var title))
        {
            return title;
        }
        return defaultTitle;
    }

    // 格式化數值顯示
    public string FormatDecimal(object? value, int decimals = 2)
    {
        if (value == null || value == DBNull.Value) return "0.00";

        if (value is System.Text.Json.JsonElement je)
        {
            if (je.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                return je.GetDecimal().ToString($"N{decimals}");
            }
            if (je.ValueKind == System.Text.Json.JsonValueKind.Null || je.ValueKind == System.Text.Json.JsonValueKind.Undefined)
            {
                return "";
            }
        }

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue) || stringValue == "{}" || stringValue == "[]")
        {
            return "";
        }

        if (decimal.TryParse(stringValue, out var dec))
        {
            return dec.ToString($"N{decimals}");
        }

        return "";
    }

    // 格式化日期顯示
    public string FormatDate(object? value)
    {
        if (value == null || value == DBNull.Value) return "";

        if (value is System.Text.Json.JsonElement je)
        {
            if (je.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                if (DateTime.TryParse(je.GetString(), out var dt))
                {
                    return dt.ToString("yyyy-MM-dd");
                }
            }
        }
        else if (value is DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }
        else if (DateTime.TryParse(value.ToString(), out var parsedDt))
        {
            return parsedDt.ToString("yyyy-MM-dd");
        }

        return value.ToString() ?? "";
    }

    // 取得字典值
    public string GetValue(Dictionary<string, object>? dict, string key, string defaultValue = "")
    {
        if (dict == null || !dict.ContainsKey(key)) return defaultValue;

        var value = dict[key];
        if (value == null || value == DBNull.Value) return defaultValue;

        if (value is System.Text.Json.JsonElement je)
        {
            return je.ValueKind switch
            {
                System.Text.Json.JsonValueKind.String => je.GetString() ?? defaultValue,
                System.Text.Json.JsonValueKind.Number => je.GetDecimal().ToString(),
                System.Text.Json.JsonValueKind.True => "是",
                System.Text.Json.JsonValueKind.False => "否",
                System.Text.Json.JsonValueKind.Null => defaultValue,
                System.Text.Json.JsonValueKind.Undefined => defaultValue,
                _ => defaultValue
            };
        }

        var stringValue = value.ToString() ?? defaultValue;

        // 避免顯示 {} 或其他不需要的符號
        if (string.IsNullOrWhiteSpace(stringValue) || stringValue == "{}" || stringValue == "[]")
        {
            return defaultValue;
        }

        return stringValue;
    }

    // 解析 JsonElement 為 Dictionary
    private Dictionary<string, object>? ParseJsonElement(System.Text.Json.JsonElement element)
    {
        if (element.ValueKind != System.Text.Json.JsonValueKind.Object)
            return null;

        var dict = new Dictionary<string, object>();
        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = ConvertJsonValue(prop.Value);
        }
        return dict;
    }

    // 解析 JsonElement 陣列
    private List<Dictionary<string, object>>? ParseJsonArray(System.Text.Json.JsonElement element)
    {
        if (element.ValueKind != System.Text.Json.JsonValueKind.Array)
            return null;

        var list = new List<Dictionary<string, object>>();
        foreach (var item in element.EnumerateArray())
        {
            var dict = ParseJsonElement(item);
            if (dict != null)
                list.Add(dict);
        }
        return list;
    }

    // 轉換 JsonElement 為適當的型別
    private object ConvertJsonValue(System.Text.Json.JsonElement element)
    {
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => element.GetString() ?? string.Empty,
            System.Text.Json.JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : element.GetDecimal(),
            System.Text.Json.JsonValueKind.True => true,
            System.Text.Json.JsonValueKind.False => false,
            System.Text.Json.JsonValueKind.Null => DBNull.Value,
            _ => element.Clone()
        };
    }
}