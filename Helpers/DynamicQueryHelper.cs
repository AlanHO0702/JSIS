using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace PcbErpApi.Helpers
{
    public static class DynamicQueryHelper
    {
        // 支援的運算符
        public enum QueryOp { Equal, NotEqual, Contains, GreaterOrEqual, LessOrEqual, Greater, Less }

        // 主入口：根據參數自動套用 Where
        public static IQueryable<T> ApplyDynamicWhere<T>(this IQueryable<T> query, List<QueryParam> filters)
        {
            foreach (var filter in filters)
            {
                if (filter == null || string.IsNullOrWhiteSpace(filter.Field) || filter.Value == null) continue;

                var property = typeof(T).GetProperty(filter.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null) continue;

                // 根據型別自動解析
                object? value = ParseValue(property.PropertyType, filter.Value);
                if (value == null) continue;

                // 動態組 Where 條件
                query = query.ApplyWhere(property, filter.Op, value);
            }
            return query;
        }

        // 單一條件
       public static IQueryable<T> ApplyWhere<T>(this IQueryable<T> query, PropertyInfo property, QueryOp op, object value)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var propExpr = Expression.Property(param, property);

            var actualType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var valExpr = Expression.Constant(value, actualType);

            Expression? body = null;

            if (actualType == typeof(string))
            {
                // string用string.Compare做大小比較
                var compareMethod = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) });
                if (compareMethod == null)
                    throw new InvalidOperationException("string.Compare(string, string) method not found.");
                var compareCall = Expression.Call(compareMethod, propExpr, valExpr);
                var zero = Expression.Constant(0);

                switch (op)
                {
                    case QueryOp.Equal:
                        body = Expression.Equal(propExpr, valExpr);
                        break;
                    case QueryOp.NotEqual:
                        body = Expression.NotEqual(propExpr, valExpr);
                        break;
                    case QueryOp.GreaterOrEqual:
                        body = Expression.GreaterThanOrEqual(compareCall, zero);
                        break;
                    case QueryOp.LessOrEqual:
                        body = Expression.LessThanOrEqual(compareCall, zero);
                        break;
                    case QueryOp.Greater:
                        body = Expression.GreaterThan(compareCall, zero);
                        break;
                    case QueryOp.Less:
                        body = Expression.LessThan(compareCall, zero);
                        break;
                    case QueryOp.Contains:
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        if (containsMethod == null)
                            throw new InvalidOperationException("string.Contains(string) method not found.");
                        body = Expression.Call(propExpr, containsMethod, valExpr);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                // 非 string，按原邏輯處理
                switch (op)
                {
                    case QueryOp.Equal:
                        body = Expression.Equal(propExpr, valExpr);
                        break;
                    case QueryOp.NotEqual:
                        body = Expression.NotEqual(propExpr, valExpr);
                        break;
                    case QueryOp.GreaterOrEqual:
                        body = Expression.GreaterThanOrEqual(propExpr, valExpr);
                        break;
                    case QueryOp.LessOrEqual:
                        body = Expression.LessThanOrEqual(propExpr, valExpr);
                        break;
                    case QueryOp.Greater:
                        body = Expression.GreaterThan(propExpr, valExpr);
                        break;
                    case QueryOp.Less:
                        body = Expression.LessThan(propExpr, valExpr);
                        break;
                    case QueryOp.Contains:
                        throw new InvalidOperationException("非 string 欄位不支援 Contains");
                    default:
                        throw new NotImplementedException();
                }
            }

            if (body == null)
                throw new InvalidOperationException("Failed to build dynamic query expression.");
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.Where(lambda);
        }

        // 解析值
        private static object? ParseValue(Type targetType, string strVal)
        {
            try
            {
                if (targetType == typeof(string))
                    return strVal;
                if (targetType == typeof(int))
                    return int.Parse(strVal);
                if (targetType == typeof(decimal))
                    return decimal.Parse(strVal);
                if (targetType == typeof(DateTime))
                    return DateTime.Parse(strVal);
                if (targetType == typeof(bool))
                    return bool.Parse(strVal);
                // Nullable<T>
                var underlying = Nullable.GetUnderlyingType(targetType);
                if (underlying != null)
                    return ParseValue(underlying, strVal);
            }
            catch { }
            return null;
        }

        // 查詢參數結構
        public class QueryParam
        {
            public string Field { get; set; }
            public QueryOp Op { get; set; }
            public string Value { get; set; }
        }

        public static QueryOp ParseOp(string op)
        {
            switch(op?.ToLower())
            {
                case "=":
                case "eq":
                    return QueryOp.Equal;
                case "!=":
                case "<>":
                case "neq":
                    return QueryOp.NotEqual;
                case ">":
                    return QueryOp.Greater;
                case ">=":
                    return QueryOp.GreaterOrEqual;
                case "<":
                    return QueryOp.Less;
                case "<=":
                    return QueryOp.LessOrEqual;
                case "like":
                case "contains":
                    return QueryOp.Contains;
                default:
                    return QueryOp.Equal;
            }
        }

    }
}
