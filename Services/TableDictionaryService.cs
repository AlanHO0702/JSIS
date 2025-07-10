using System;
using System.Collections.Generic;
using System.Linq;
using PcbErpApi.Data;
using PcbErpApi.Models;

public interface ITableDictionaryService
{
    List<CURdTableField> GetFieldDict(string tableName, Type modelType);
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
}
