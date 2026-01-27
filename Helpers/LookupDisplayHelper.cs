using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PcbErpApi.Models;

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
    object? headerData,
    IEnumerable<dynamic>? lookupMaps
)
{
    var dict = new Dictionary<string, string>();
    if (headerData == null || lookupMaps == null)
        return dict;

    IDictionary<string, object>? d = headerData as IDictionary<string, object>;

    foreach (var map in lookupMaps)
    {
        object? keyValueObj = null;
        string? foundKey = null;
        string? fieldName = map.FieldName;
        if (string.IsNullOrWhiteSpace(fieldName))
            continue;

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
        if (string.IsNullOrWhiteSpace(keyValue))
            continue;

        var lookupValues = map.LookupValues as IDictionary<string, string>;
        if (lookupValues == null || lookupValues.Count == 0)
            continue;

        if (lookupValues.TryGetValue(keyValue, out var display))
        {
            dict[fieldName] = display;
        }
        else
        {
            foreach (var kv in lookupValues)
            {
                if (string.Equals(kv.Key, keyValue, StringComparison.OrdinalIgnoreCase))
                {
                    dict[fieldName] = kv.Value;
                    break;
                }
            }
        }

    }

    return dict;
}

    public static Dictionary<string, string> BuildHeaderLookupMapFromStandard(
        object? headerData,
        IEnumerable<TableFieldViewModel>? fields,
        DbConnection? conn)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (headerData == null || fields == null || conn == null)
            return dict;

        IDictionary<string, object>? dataDict = headerData as IDictionary<string, object>;

        static bool IsSafeName(string name)
            => Regex.IsMatch(name ?? "", @"^[A-Za-z0-9_]+$");

        static bool IsSafeTable(string table)
            => (table ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries)
                .All(part => IsSafeName(part.Trim('[', ']')));

        static string Esc(string ident) => $"[{ident.Replace("]", "]]")}]";

        static string EscTable(string raw)
        {
            var parts = (raw ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(".", parts.Select(p => Esc(p.Trim('[', ']'))));
        }

        static bool TryGetValueIgnoreCase(IDictionary<string, object> dict, string key, out object? value)
        {
            if (dict.TryGetValue(key, out value)) return true;
            var hit = dict.Keys.FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
            if (hit == null) { value = null; return false; }
            value = dict[hit];
            return true;
        }

        var shouldClose = conn.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            conn.Open();

        try
        {
            foreach (var field in fields)
            {
                if (field == null || string.IsNullOrWhiteSpace(field.FieldName)) continue;
                var fieldName = field.FieldName;
                var lookupTable = field.LookupTable;
                var lookupKey = field.LookupKeyField;
                var lookupResult = field.LookupResultField;
                if (string.IsNullOrWhiteSpace(lookupTable)
                    || string.IsNullOrWhiteSpace(lookupKey)
                    || string.IsNullOrWhiteSpace(lookupResult))
                    continue;

                if (!IsSafeTable(lookupTable) || !IsSafeName(lookupKey))
                    continue;

                object? keyValueObj = null;
                if (dataDict != null)
                {
                    if (!TryGetValueIgnoreCase(dataDict, fieldName, out keyValueObj))
                        continue;
                }
                else
                {
                    keyValueObj = headerData.GetType().GetProperty(fieldName)?.GetValue(headerData);
                }

                var keyValue = keyValueObj?.ToString();
                if (string.IsNullOrWhiteSpace(keyValue))
                    continue;

                var resultFields = lookupResult.Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
                if (resultFields.Length == 0 || resultFields.Any(r => !IsSafeName(r)))
                    continue;

                var select = string.Join(", ", resultFields.Select((r, i) => $"{Esc(r)} AS [r{i}]"));
                var sql = $"SELECT TOP 1 {select} FROM {EscTable(lookupTable)} WHERE {Esc(lookupKey)} = @key";

                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var p = cmd.CreateParameter();
                p.ParameterName = "@key";
                p.Value = keyValue;
                cmd.Parameters.Add(p);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) continue;

                var parts = new List<string>();
                for (var i = 0; i < resultFields.Length; i++)
                {
                    var val = reader[$"r{i}"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(val))
                        parts.Add(val);
                }
                if (parts.Count == 0) continue;
                dict[fieldName] = string.Join(" - ", parts);
            }
        }
        finally
        {
            if (shouldClose) conn.Close();
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
                if (item == null) continue;
                var rowKey = keySelector(item);
                if (string.IsNullOrEmpty(rowKey)) continue;

                // 這邊直接複用單筆 lookup 對應
                result[rowKey] = BuildHeaderLookupMap(item, lookupMaps);
            }
            return result;
        }

    }
}
