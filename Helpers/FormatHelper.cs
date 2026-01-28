using System;
using System.Globalization;
using System.Text.Json;

public static class FormatHelper
{
    public static string FormatValue(object? rawValue, string? dataType, string? formatStr)
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
                var dateFormat = NormalizeDateFormatMask(formatStr);
                if (rawValue is DateTime dt)
                {
                    return dt.ToString(dateFormat);
                }
                var rawText = rawValue.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(rawText))
                {
                    var normalized = rawText.Replace("上午", "AM").Replace("下午", "PM");
                    if (DateTime.TryParse(rawText, CultureInfo.GetCultureInfo("zh-TW"), DateTimeStyles.AllowWhiteSpaces, out var parsedTw)
                        || DateTime.TryParse(normalized, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces, out parsedTw)
                        || DateTime.TryParse(rawText, out parsedTw))
                    {
                        return parsedTw.ToString(dateFormat);
                    }
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
                    var formatted = number.ToString(fmt);
                    return TrimZeroDecimal(formatted);
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

    private static string NormalizeDateFormatMask(string formatStr)
    {
        if (string.IsNullOrWhiteSpace(formatStr)) return formatStr;

        var fmt = formatStr;
        var timeIdx = IndexOfTimeToken(fmt);
        var datePart = timeIdx >= 0 ? fmt.Substring(0, timeIdx) : fmt;
        var timePart = timeIdx >= 0 ? fmt.Substring(timeIdx) : string.Empty;

        datePart = NormalizeMonthToken(datePart);
        timePart = NormalizeMinuteToken(timePart);

        return datePart + timePart;
    }

    private static int IndexOfTimeToken(string fmt)
    {
        if (string.IsNullOrEmpty(fmt)) return -1;
        for (var i = 0; i < fmt.Length; i++)
        {
            var c = fmt[i];
            if (c == 'h' || c == 'H') return i;
        }
        return -1;
    }

    private static string NormalizeMonthToken(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var chars = input.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] == 'm') chars[i] = 'M';
        }
        return new string(chars);
    }

    private static string NormalizeMinuteToken(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var chars = input.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] == 'n' || chars[i] == 'N') chars[i] = 'm';
        }
        return new string(chars);
    }

    private static string TrimZeroDecimal(string formatted)
    {
        if (string.IsNullOrWhiteSpace(formatted)) return formatted;
        var sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var idx = formatted.IndexOf(sep, StringComparison.Ordinal);
        if (idx < 0) return formatted;
        var end = formatted.Length - 1;
        while (end > idx && formatted[end] == '0') end--;
        if (end == idx) return formatted.Substring(0, idx);
        return formatted.Substring(0, end + 1);
    }

    private static string NormalizeDataType(string? dataType, object rawValue)
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
