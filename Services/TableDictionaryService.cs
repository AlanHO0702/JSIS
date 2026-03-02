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

            var ocxTableClean = Clean(ocxTableName ?? "");
            var resultType = _context.CURdTableFields
                .Where(x => x.TableName != null
                            && (x.TableName.ToLower() == ocxTableClean
                                || x.TableName.ToLower().Replace("dbo.", "") == ocxTableClean)
                            && x.FieldName == ocxResultName)
                .Select(x => x.DataType)
                .FirstOrDefault();

            Dictionary<string, string>? lookupDict = null;
            string selectedKeyField = "";
            string selectedKeySelf = "";

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

            if (lookupDict == null || lookupDict.Count == 0)
                continue;

            result.Add(new OCXLookupMap
            {
                FieldName = fieldName,
                KeySelfName = selectedKeySelf,
                KeyFieldName = selectedKeyField,
                LookupValues = lookupDict,
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
        public string? ResultDataType { get; set; }
    }
}
