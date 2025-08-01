using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PcbErpApi.Helpers
{
    public static class LookupDisplayHelper
    {
        // // 取顯示值
        public static string? GetLookupDisplay(this ViewDataDictionary viewData, string masterKey, string fieldName)
        {
            if (viewData["LookupDisplayMap"] is Dictionary<string, Dictionary<string, string>> lookupMap &&
                lookupMap.TryGetValue(masterKey, out var dict) &&
                dict.TryGetValue(fieldName, out var val))
            {
                return val;
            }
            return null;
        }

        // 新增支援指定 lookup map 名稱
        public static string? GetLookupDisplay(this ViewDataDictionary viewData, string masterKey, string fieldName, string mapName)
        {
            if (viewData[mapName] is Dictionary<string, Dictionary<string, string>> lookupMap &&
                lookupMap.TryGetValue(masterKey, out var dict) &&
                dict.TryGetValue(fieldName, out var val))
            {
                return val;
            }
            return null;
        }

        // 產生單頭 lookup display 字典
public static Dictionary<string, string> BuildHeaderLookupMap(
    object headerData,
    IEnumerable<dynamic> lookupMaps
)
{
    var dict = new Dictionary<string, string>();
    if (headerData == null || lookupMaps == null)
        return dict;

    IDictionary<string, object> d = headerData as IDictionary<string, object>;

    foreach (var map in lookupMaps)
    {
        object keyValueObj = null;
        string foundKey = null;

        if (d != null)
        {
            // ★★★ 忽略大小寫比對 key ★★★
            foundKey = d.Keys.FirstOrDefault(k => string.Equals(k, map.KeySelfName, StringComparison.OrdinalIgnoreCase));
            if (foundKey != null)
                keyValueObj = d[foundKey];
        }
        else
        {
            keyValueObj = headerData.GetType().GetProperty(map.KeySelfName)?.GetValue(headerData);
        }

            var keyValue = keyValueObj?.ToString();
            string display = null; // ★★★ 先宣告

            if (!string.IsNullOrEmpty(keyValue) && map.LookupValues.TryGetValue(keyValue, out display))
            {
                dict[map.FieldName] = display;
            }

    }

    return dict;
}

        /// <summary>
        /// 多筆資料通用 lookup display map 建立
        /// </summary>
        /// <typeparam name="T">明細型別</typeparam>
        /// <param name="items">明細資料清單</param>
        /// <param name="lookupMaps">Lookup 設定清單</param>
        /// <param name="keySelector">指定主鍵產生規則，例如 item => $"{PaperNum}_{Item}"</param>
        /// <returns>回傳 Dictionary<rowKey, Dictionary<欄位, 顯示值>></returns>
        public static Dictionary<string, Dictionary<string, string>> BuildLookupDisplayMap<T>(
            IEnumerable<T> items,
            IEnumerable<dynamic> lookupMaps,
            Func<T, string> keySelector
        )
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach (var item in items)
            {
                var rowKey = keySelector(item);
                if (string.IsNullOrEmpty(rowKey)) continue;

                // 這邊直接複用單筆 lookup 對應
                result[rowKey] = BuildHeaderLookupMap(item, lookupMaps);
            }
            return result;
        }

    }
}
