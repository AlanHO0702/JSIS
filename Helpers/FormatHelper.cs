using System;
using System.Text.Json;

public static class FormatHelper
{
    public static string FormatValue(object rawValue, string dataType, string formatStr)
    {
        if (rawValue == null) return "";

        // System.Text.Json 反序列化到 object 時常見會是 JsonElement
        if (rawValue is JsonElement je)
        {
            rawValue = je.ValueKind switch
            {
                JsonValueKind.String => je.GetString() ?? "",
                JsonValueKind.Number => je.TryGetDecimal(out var dec) ? dec : (je.TryGetDouble(out var dbl) ? dbl : je.ToString()),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => "",
                JsonValueKind.Undefined => "",
                _ => je.ToString()
            };
        }

        var normalizedType = NormalizeDataType(dataType, rawValue);

        if (!string.IsNullOrEmpty(formatStr))
        {
            // 日期格式
            if (normalizedType == "date")
            {
                if (rawValue is DateTime dt)
                {
                    return dt.ToString(formatStr.Replace("nn", "mm"));
                }
                if (DateTime.TryParse(rawValue.ToString(), out var parsedDt))
                {
                    return parsedDt.ToString(formatStr.Replace("nn", "mm"));
                }
            }
            // 數字格式
            else if (normalizedType == "number" || normalizedType == "")
            {
                try
                {
                    decimal number;

                    if (rawValue is string s)
                    {
                        if (!decimal.TryParse(s, out number))
                            return s; // 不是數字 → 原樣返回
                    }
                    else
                    {
                        number = Convert.ToDecimal(rawValue);
                    }

                    var fmt = formatStr;
                    if (fmt.StartsWith("."))
                        fmt = "0" + fmt;
                    return number.ToString(fmt);
                }
                catch
                {
                    return rawValue?.ToString() ?? string.Empty;
                }
            }
        }

        // 預設
        return rawValue?.ToString() ?? string.Empty;
    }

    private static string NormalizeDataType(string dataType, object rawValue)
    {
        // 1) 先依 rawValue 型別推斷
        if (rawValue is DateTime) return "date";
        if (rawValue is sbyte or byte or short or ushort or int or uint or long or ulong
            or float or double or decimal) return "number";

        // 2) 再依 DataType 字串推斷（SQL 型別/自訂型別常會長得像 decimal(24,8)、datetime）
        var dt = (dataType ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(dt)) return "";

        if (dt is "date" or "datetime" or "smalldatetime" or "datetime2" or "datetimeoffset" or "time")
            return "date";

        if (dt.StartsWith("date") || dt.Contains("datetime") || dt.Contains("time"))
            return "date";

        if (dt is "number" or "int" or "smallint" or "tinyint" or "bigint"
            or "decimal" or "numeric" or "money" or "smallmoney" or "float" or "real")
            return "number";

        if (dt.StartsWith("decimal") || dt.StartsWith("numeric") || dt.StartsWith("money") || dt.StartsWith("smallmoney")
            || dt.StartsWith("float") || dt.StartsWith("real")
            || dt.StartsWith("int") || dt.StartsWith("smallint") || dt.StartsWith("tinyint") || dt.StartsWith("bigint"))
            return "number";

        return dt;
    }
}
