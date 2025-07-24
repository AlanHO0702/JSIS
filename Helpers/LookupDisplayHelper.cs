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
            foreach (var map in lookupMaps)
            {
                object keyValueObj = null;
                if (headerData is IDictionary<string, object> d)
                    d.TryGetValue(map.KeySelfName, out keyValueObj);
                else
                    keyValueObj = headerData.GetType().GetProperty(map.KeySelfName)?.GetValue(headerData);

                var keyValue = keyValueObj?.ToString();
                if (!string.IsNullOrEmpty(keyValue))
                {
                    string display; // 這裡要明確指定型別
                    if (map.LookupValues.TryGetValue(keyValue, out display))
                    {
                        dict[map.FieldName] = display;
                    }
                }
            }

            return dict;
        }

    }
}
