using System;
using System.Collections.Generic;
using System.Linq;
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

    public List<CURdTableField> GetFieldDict(string tableName, Type modelType)
    {
        // 取得 model 各屬性型別
        var modelFieldTypes = modelType
            .GetProperties()
            .ToDictionary(p => p.Name, p => GetInputType(p.PropertyType));

        var fields = _context.CURdTableFields
            .Where(x => x.TableName == tableName)
            .OrderBy(x => x.SerialNum)
            .ToList();

        // 語系欄位設定（目前預設 TW；不存在就略過）
        var langRows = _context.CurdTableFieldLangs
            .Where(x => x.TableName == tableName && x.LanguageId == "TW")
            .ToList();

        if (langRows.Count > 0)
        {
            var langMap = langRows
                .GroupBy(x => x.FieldName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var f in fields)
            {
                if (f?.FieldName == null) continue;
                if (!langMap.TryGetValue(f.FieldName, out var l)) continue;

                // 以語系表為準（通常 UI 位置/顯示寬度都在 Lang 表調整）
                if (!string.IsNullOrWhiteSpace(l.DisplayLabel))
                    f.DisplayLabel = l.DisplayLabel;

                if (l.DisplaySize != null && l.DisplaySize > 0)
                    f.DisplaySize = l.DisplaySize;

                if (l.IFieldWidth > 0)
                    f.iFieldWidth = l.IFieldWidth;
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
        var fieldDefs = _context.CURdTableFields
            .Where(x => x.TableName == tableName
                        && !string.IsNullOrWhiteSpace(x.OCXLKTableName)
                        && !string.IsNullOrWhiteSpace(x.OCXLKResultName))
            .ToList();

        var result = new List<OCXLookupMap>();

        static string Q(string ident) => $"[{ident.Replace("]", "]]")}]";

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
            var lkSetting = _context.CURdOCXTableFieldLK
                .FirstOrDefault(x => x.TableName == tableName && x.FieldName == field.FieldName);

                if (lkSetting == null) continue;

            var ocxTableName = field.OCXLKTableName;
            var ocxResultName = field.OCXLKResultName;
            var keyField = lkSetting.KeyFieldName;

            if (HasInvalidLookupSetting(ocxTableName, keyField, ocxResultName))
                continue;

            // 用 raw SQL 動態查表（用固定 alias，避免欄名空白/重複/特殊字元造成 reader 取值失敗）
            var sql = $"SELECT {Q(keyField!)} AS [__k], {Q(ocxResultName!)} AS [__v] FROM {Q(ocxTableName!)}";
            var lookupDict = new Dictionary<string, string>();

            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var key = reader["__k"]?.ToString();
                            var value = reader["__v"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(key) && value != null)
                            {
                                lookupDict[key] = value;
                            }
                        }
                    }
                }
            }
            catch
            {
                // 當 lookup 設定不完整/查表失敗時，不讓整頁爆炸，直接略過該欄位的 lookup
                continue;
            }

            result.Add(new OCXLookupMap
            {
                FieldName = field.FieldName,
                KeySelfName = lkSetting.KeySelfName,
                KeyFieldName = keyField,
                LookupValues = lookupDict
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
    }
}
