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
            .Where(x => x.TableName == tableName && x.OCXLKTableName != null && x.OCXLKResultName != null)
            .ToList();

        var result = new List<OCXLookupMap>();

        foreach (var field in fieldDefs)
        {
            var lkSetting = _context.CURdOCXTableFieldLK
                .FirstOrDefault(x => x.TableName == tableName && x.FieldName == field.FieldName);

            if (lkSetting == null) continue;

            var ocxTableName = field.OCXLKTableName;
            var resultField = field.OCXLKResultName;
            var keyField = lkSetting.KeyFieldName;

            // 用 raw SQL 動態查表
            var sql = $"SELECT [{keyField}], [{resultField}] FROM [{ocxTableName}]";
            var conn = _context.Database.GetDbConnection();
            var lookupDict = new Dictionary<string, string>();

            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var key = reader[keyField]?.ToString();
                        var value = reader[resultField]?.ToString();
                        if (key != null && value != null)
                        {
                            lookupDict[key] = value;
                        }
                    }
                }
            }
            conn.Close();

            result.Add(new OCXLookupMap
            {
                FieldName = field.FieldName,
                KeySelfName = lkSetting.KeySelfName,
                LookupValues = lookupDict
            });
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
        public Dictionary<string, string> LookupValues { get; set; } = new();
    }
}
