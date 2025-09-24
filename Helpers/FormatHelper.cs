public static class FormatHelper
{
    public static string FormatValue(object rawValue, string dataType, string formatStr)
    {
        if (rawValue == null) return "";

        if (!string.IsNullOrEmpty(formatStr))
        {
            // 日期格式
            if (dataType?.ToLower() == "date")
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
            else if (dataType?.ToLower() == "number")
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

                    return number.ToString(formatStr);
                }
                catch
                {
                    return rawValue.ToString();
                }
            }
        }

        // 預設
        return rawValue.ToString();
    }
}
