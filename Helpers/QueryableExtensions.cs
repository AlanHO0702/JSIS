using System;
using System.Linq;
using System.Linq.Expressions;

namespace PcbErpApi.Helpers // 你的 namespace
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereDynamicStringContains<T>(this IQueryable<T> query, string propName, string value)
        {
            var param = Expression.Parameter(typeof(T));
            var property = Expression.Property(param, propName);
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var valueExpr = Expression.Constant(value, typeof(string));
            var body = Expression.Call(property, method, valueExpr);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.Where(lambda);
        }

        public static IQueryable<T> WhereDynamicDecimalEquals<T>(this IQueryable<T> query, string propName, decimal value)
        {
            var param = Expression.Parameter(typeof(T));
            var property = Expression.Property(param, propName);
            var valueExpr = Expression.Constant(value, typeof(decimal));
            var body = Expression.Equal(property, valueExpr);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.Where(lambda);
        }

        public static IQueryable<T> WhereDateGreaterThan<T>(this IQueryable<T> query, string propName, DateTime dt)
        {
            var param = Expression.Parameter(typeof(T));
            var property = Expression.Property(param, propName);
            var valueExpr = Expression.Constant(dt, typeof(DateTime));
            var body = Expression.GreaterThanOrEqual(property, valueExpr);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.Where(lambda);
        }

        public static IQueryable<T> WhereDateLessThanOrEqual<T>(this IQueryable<T> query, string propName, DateTime dt)
        {
            var param = Expression.Parameter(typeof(T));
            var property = Expression.Property(param, propName);
            var valueExpr = Expression.Constant(dt, typeof(DateTime));
            var body = Expression.LessThanOrEqual(property, valueExpr);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.Where(lambda);
        }
        public static IQueryable<T> WhereDynamicDateEquals<T>(this IQueryable<T> query, string propName, DateTime value)
        {
            var param = Expression.Parameter(typeof(T));
            var property = Expression.Property(param, propName);
            var valueExpr = Expression.Constant(value, typeof(DateTime));
            var body = Expression.Equal(property, valueExpr);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.Where(lambda);
        }

    }
}
