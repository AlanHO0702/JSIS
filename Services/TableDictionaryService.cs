using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using static TableDictionaryService;

public interface ITableDictionaryService
{
    List<CURdTableField> GetFieldDict(string tableName, Type modelType);
    List<OCXLookupMap> GetOCXLookups(string tableName);
    
}

public class TableDictionaryService : ITableDictionaryService
{
    private readonly PcbErpContext _context;
    private const char CompositeKeySeparator = '\u001F';

    public TableDictionaryService(PcbErpContext context)
    {
        _context = context;
    }

    private static string Clean(string s) => (s ?? "")
        .Trim().Trim('[', ']')
        .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase)
        .ToLowerInvariant();

    private static int GetKeySelfRank(string? keySelfName, string? fieldName)
    {
        var keySelf = (keySelfName ?? "").Trim();
        var field = (fieldName ?? "").Trim();

        if (string.IsNullOrWhiteSpace(keySelf) || string.IsNullOrWhiteSpace(field)) return 9;
        if (string.Equals(keySelf, field, StringComparison.OrdinalIgnoreCase)) return 0;

        if (field.EndsWith("Name", StringComparison.OrdinalIgnoreCase) && field.Length > 4)
        {
            var baseName = field.Substring(0, field.Length - 4);
            if (string.Equals(keySelf, baseName + "Id", StringComparison.OrdinalIgnoreCase)) return 1;
            if (string.Equals(keySelf, baseName, StringComparison.OrdinalIgnoreCase)) return 2;
        }

        return 5;
    }

    private static bool TryLoadLookupValues(
        DbConnection conn,
        string lookupTableName,
        string keyFieldName,
        string resultFieldName,
        out Dictionary<string, string> lookupDict)
    {
        lookupDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static string Q(string ident) => $"[{ident.Replace("]", "]]")}]";
        var sql = $"SELECT {Q(keyFieldName)} AS [__k], {Q(resultFieldName)} AS [__v] FROM {Q(lookupTableName)}";

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var key = reader["__k"]?.ToString();
                var value = reader["__v"]?.ToString();
                var normalizedKey = key?.Trim();
                if (!string.IsNullOrWhiteSpace(normalizedKey) && value != null)
                {
                    lookupDict[normalizedKey.ToLowerInvariant()] = value;
                }
            }

            return lookupDict.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryLoadCompositeLookupValues(
        DbConnection conn,
        string lookupTableName,
        IReadOnlyList<string> keyFieldNames,
        string resultFieldName,
        out Dictionary<string, string> lookupDict)
    {
        lookupDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (keyFieldNames == null || keyFieldNames.Count == 0)
            return false;

        static string Q(string ident) => $"[{ident.Replace("]", "]]")}]";
        var keySelect = string.Join(", ", keyFieldNames.Select((x, i) => $"{Q(x)} AS [__k{i}]"));
        var sql = $"SELECT {keySelect}, {Q(resultFieldName)} AS [__v] FROM {Q(lookupTableName)}";

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var keyParts = new List<string>(keyFieldNames.Count);
                var hasEmptyPart = false;

                for (var i = 0; i < keyFieldNames.Count; i++)
                {
                    var keyPart = NormalizeLookupKey(reader[$"__k{i}"]?.ToString());
                    if (string.IsNullOrWhiteSpace(keyPart))
                    {
                        hasEmptyPart = true;
                        break;
                    }
                    keyParts.Add(keyPart);
                }

                if (hasEmptyPart)
                    continue;

                var value = reader["__v"]?.ToString();
                if (value == null)
                    continue;

                lookupDict[BuildCompositeKey(keyParts)] = value;
            }

            return lookupDict.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeLookupKey(string? raw)
        => (raw ?? "").Trim().ToLowerInvariant();

    private static string BuildCompositeKey(IEnumerable<string> keyParts)
        => string.Join(CompositeKeySeparator, keyParts.Select(NormalizeLookupKey));

    private static bool TryGetSourceValue(IReadOnlyDictionary<string, object?> source, string? fieldName, out string value)
    {
        value = "";
        if (string.IsNullOrWhiteSpace(fieldName))
            return false;

        if (source.TryGetValue(fieldName, out var raw))
        {
            value = NormalizeLookupKey(raw?.ToString());
            return !string.IsNullOrWhiteSpace(value);
        }

        foreach (var kv in source)
        {
            if (!string.Equals(kv.Key, fieldName, StringComparison.OrdinalIgnoreCase))
                continue;
            value = NormalizeLookupKey(kv.Value?.ToString());
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }

    public static string ResolveLookupDisplay(OCXLookupMap map, IReadOnlyDictionary<string, object?> source)
    {
        if (map == null || source == null || string.IsNullOrWhiteSpace(map.FieldName))
            return "";

        // 複合鍵設定時，先用複合鍵比對，避免只用單一欄位誤配到其他單據
        if (map.CompositeLookupValues != null
            && map.CompositeLookupValues.Count > 0
            && map.CompositeKeyPairs != null
            && map.CompositeKeyPairs.Count > 1)
        {
            var keyParts = new List<string>(map.CompositeKeyPairs.Count);
            foreach (var pair in map.CompositeKeyPairs)
            {
                var sourceField = !string.IsNullOrWhiteSpace(pair.KeySelfName)
                    ? pair.KeySelfName
                    : pair.KeyFieldName;
                if (!TryGetSourceValue(source, sourceField, out var part))
                {
                    keyParts.Clear();
                    break;
                }
                keyParts.Add(part);
            }

            if (keyParts.Count == map.CompositeKeyPairs.Count)
            {
                var compositeKey = BuildCompositeKey(keyParts);
                if (map.CompositeLookupValues.TryGetValue(compositeKey, out var compositeDisplay) && compositeDisplay != null)
                    return compositeDisplay;
            }

            // 有設定複合鍵但比不到時，避免退回單鍵導致錯誤值
            return "";
        }

        var key = "";
        if (TryGetSourceValue(source, map.KeyFieldName, out var keyFieldValue))
            key = keyFieldValue;
        if (string.IsNullOrWhiteSpace(key) && TryGetSourceValue(source, map.KeySelfName, out var keySelfValue))
            key = keySelfValue;
        if (string.IsNullOrWhiteSpace(key) && TryGetSourceValue(source, map.FieldName, out var rawValue))
            key = rawValue;

        if (!string.IsNullOrWhiteSpace(key)
            && map.LookupValues != null
            && map.LookupValues.TryGetValue(key, out var display)
            && display != null)
            return display;

        return "";
    }

    public List<CURdTableField> GetFieldDict(string tableName, Type modelType)
    {
        var tname = Clean(tableName);

        // 取得 model 各屬性型別
        var modelFieldTypes = modelType
            .GetProperties()
            .ToDictionary(p => p.Name, p => GetInputType(p.PropertyType));

        var rawFields = _context.CURdTableFields
            .Where(x => x.TableName != null
                        && (x.TableName.ToLower() == tname
                            || x.TableName.ToLower().Replace("dbo.", "") == tname))
            .OrderBy(x => x.SerialNum)
            .ToList();

        var fields = rawFields
            .OrderBy(x => string.Equals(Clean(x.TableName ?? ""), tname, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(x => x.SerialNum ?? int.MaxValue)
            .ThenBy(x => x.FieldName)
            .GroupBy(x => x.FieldName ?? "", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        // 語系欄位設定（目前預設 TW；不存在就略過）
        var langRows = _context.CurdTableFieldLangs
            .Where(x => x.LanguageId == "TW"
                        && x.TableName != null
                        && (x.TableName.ToLower() == tname
                            || x.TableName.ToLower().Replace("dbo.", "") == tname))
            .ToList();

        if (langRows.Count > 0)
        {
            var langMap = langRows
                .Where(x => !string.IsNullOrWhiteSpace(x.FieldName))
                .GroupBy(x => x.FieldName!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => string.Equals(Clean(x.TableName ?? ""), tname, StringComparison.OrdinalIgnoreCase) ? 0 : 1).First(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var f in fields)
            {
                if (f?.FieldName == null) continue;
                if (!langMap.TryGetValue(f.FieldName, out var l)) continue;

                // 以語系表為準（通常 UI 位置/顯示寬度都在 Lang 表調整）
                if (!string.IsNullOrWhiteSpace(l.DisplayLabel))
                    f.DisplayLabel = l.DisplayLabel;

                if (l.DisplaySize != null && l.DisplaySize > 0)
                    f.DisplaySize = l.DisplaySize;

                if ((f.iFieldWidth ?? 0) <= 0 && l.IFieldWidth > 0)
                    f.iFieldWidth = l.IFieldWidth;
                if (string.IsNullOrWhiteSpace(f.EditColor) && !string.IsNullOrWhiteSpace(l.EditColor))
                    f.EditColor = l.EditColor;
            }
        }

        // 資料庫沒填型別時，用 model 的型別補上
        foreach (var f in fields)
        {
            if (string.IsNullOrWhiteSpace(f.DataType) && modelFieldTypes.TryGetValue(f.FieldName, out var dt))
            {
                f.DataType = dt;
            }

        }
        return fields;
    }

    public List<OCXLookupMap> GetOCXLookups(string tableName)
    {
        var tname = Clean(tableName);

        var rawFieldDefs = _context.CURdTableFields
            .Where(x => x.TableName != null
                        && (x.TableName.ToLower() == tname
                            || x.TableName.ToLower().Replace("dbo.", "") == tname)
                        && !string.IsNullOrWhiteSpace(x.OCXLKTableName)
                        && !string.IsNullOrWhiteSpace(x.OCXLKResultName))
            .ToList();

        var fieldDefs = rawFieldDefs
            .OrderBy(x => string.Equals(Clean(x.TableName ?? ""), tname, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(x => x.SerialNum ?? int.MaxValue)
            .ThenBy(x => x.FieldName)
            .GroupBy(x => x.FieldName ?? "", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        var result = new List<OCXLookupMap>();

        static bool HasInvalidLookupSetting(string? table, string? keyField, string? resultField)
        {
            return string.IsNullOrWhiteSpace(table)
                   || string.IsNullOrWhiteSpace(keyField)
                   || string.IsNullOrWhiteSpace(resultField);
        }

        var conn = _context.Database.GetDbConnection();
        var shouldClose = false;
        if (conn.State != System.Data.ConnectionState.Open)
        {
            conn.Open();
            shouldClose = true;
        }

        foreach (var field in fieldDefs)
        {
            var ocxTableName = field.OCXLKTableName;
            var ocxResultName = field.OCXLKResultName;
            var fieldName = field.FieldName ?? "";
            var fieldNameLower = fieldName.ToLowerInvariant();

            var lkSettings = _context.CURdOCXTableFieldLK
                .Where(x => x.TableName != null
                            && (x.TableName.ToLower() == tname
                                || x.TableName.ToLower().Replace("dbo.", "") == tname)
                            && x.FieldName != null
                            && x.FieldName.ToLower() == fieldNameLower)
                .ToList()
                .OrderBy(x => GetKeySelfRank(x.KeySelfName, fieldName))
                .ThenBy(x => x.KeyFieldName)
                .ThenBy(x => x.KeySelfName)
                .ToList();

            if (lkSettings.Count == 0)
                continue;

            if (HasInvalidLookupSetting(ocxTableName, "X", ocxResultName))
                continue;

            var compositeKeyPairs = new List<OCXLookupKeyPair>();
            var pairSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var lk in lkSettings)
            {
                var keyField = lk.KeyFieldName?.Trim() ?? "";
                var keySelf = string.IsNullOrWhiteSpace(lk.KeySelfName)
                    ? keyField
                    : lk.KeySelfName.Trim();

                if (HasInvalidLookupSetting(ocxTableName, keyField, ocxResultName)
                    || string.IsNullOrWhiteSpace(keySelf))
                    continue;

                var pairToken = $"{keyField}{CompositeKeySeparator}{keySelf}";
                if (!pairSet.Add(pairToken))
                    continue;

                compositeKeyPairs.Add(new OCXLookupKeyPair
                {
                    KeyFieldName = keyField,
                    KeySelfName = keySelf
                });
            }

            var ocxTableClean = Clean(ocxTableName ?? "");
            var resultType = _context.CURdTableFields
                .Where(x => x.TableName != null
                            && (x.TableName.ToLower() == ocxTableClean
                                || x.TableName.ToLower().Replace("dbo.", "") == ocxTableClean)
                            && x.FieldName == ocxResultName)
                .Select(x => x.DataType)
                .FirstOrDefault();

            Dictionary<string, string>? lookupDict = null;
            Dictionary<string, string>? compositeLookupDict = null;
            string selectedKeyField = "";
            string selectedKeySelf = "";

            if (compositeKeyPairs.Count > 1
                && TryLoadCompositeLookupValues(
                    conn,
                    ocxTableName!,
                    compositeKeyPairs.Select(x => x.KeyFieldName).ToList(),
                    ocxResultName!,
                    out var loadedCompositeLookupDict))
            {
                compositeLookupDict = loadedCompositeLookupDict;
            }

            foreach (var lkSetting in lkSettings)
            {
                var keyCandidates = new List<string>();
                if (!string.IsNullOrWhiteSpace(lkSetting.KeyFieldName))
                    keyCandidates.Add(lkSetting.KeyFieldName.Trim());
                if (!string.IsNullOrWhiteSpace(lkSetting.KeySelfName)
                    && !keyCandidates.Any(x => string.Equals(x, lkSetting.KeySelfName, StringComparison.OrdinalIgnoreCase)))
                    keyCandidates.Add(lkSetting.KeySelfName.Trim());

                foreach (var keyCandidate in keyCandidates)
                {
                    if (HasInvalidLookupSetting(ocxTableName, keyCandidate, ocxResultName))
                        continue;

                    if (TryLoadLookupValues(conn, ocxTableName!, keyCandidate, ocxResultName!, out var loadedLookupDict))
                    {
                        lookupDict = loadedLookupDict;
                        selectedKeyField = keyCandidate;
                        selectedKeySelf = string.IsNullOrWhiteSpace(lkSetting.KeySelfName)
                            ? keyCandidate
                            : lkSetting.KeySelfName;
                        break;
                    }
                }

                if (lookupDict != null)
                    break;
            }

            var hasSingleLookup = lookupDict != null && lookupDict.Count > 0;
            var hasCompositeLookup = compositeLookupDict != null && compositeLookupDict.Count > 0;
            if (!hasSingleLookup && !hasCompositeLookup)
                continue;

            result.Add(new OCXLookupMap
            {
                FieldName = fieldName,
                KeySelfName = selectedKeySelf,
                KeyFieldName = selectedKeyField,
                LookupValues = lookupDict ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                CompositeKeyPairs = compositeKeyPairs,
                CompositeLookupValues = compositeLookupDict ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                ResultDataType = resultType
            });
        }

        if (shouldClose)
        {
            conn.Close();
        }

        return result;
    }


    // 型別轉 input type
    private string GetInputType(Type type)
    {
        if (type == typeof(int) || type == typeof(double) || type == typeof(decimal) || type == typeof(float))
            return "number";
        if (type == typeof(DateTime))
            return "date";
        if (type == typeof(bool))
            return "checkbox";
        return "text";
    }

    public class OCXLookupMap
    {
        public string FieldName { get; set; } = null!;
        public string KeySelfName { get; set; } = null!;
        public string KeyFieldName { get; set; } = null!;
        public Dictionary<string, string> LookupValues { get; set; } = new();
        public List<OCXLookupKeyPair> CompositeKeyPairs { get; set; } = new();
        public Dictionary<string, string> CompositeLookupValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string? ResultDataType { get; set; }
    }

    public class OCXLookupKeyPair
    {
        public string KeySelfName { get; set; } = null!;
        public string KeyFieldName { get; set; } = null!;
    }
}
