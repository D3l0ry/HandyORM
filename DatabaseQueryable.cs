using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    public static class DatabaseQueryable
    {
        #region MethodHelper
        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> func, T1 unused) => func.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> func, T1 unused, T2 unused2) => func.Method;
        #endregion

        public static ITableQueryable<TSource> Where<TSource>(this ITableQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Provider
                .CreateQuery(
                    Expression.Call(
                        null,
                        GetMethodInfo(Where, source, predicate),
                        new Expression[] { source.Expression, Expression.Quote(predicate) })
                    );
        }

        public static TSource First<TSource>(this ITableQueryable<TSource> source) where TSource : class
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.Execute(
                Expression.Call(
                    null,
                    GetMethodInfo(First, source),
                    new Expression[] { source.Expression }
                    )
                ).First();
        }

        public static TSource First<TSource>(this ITableQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Provider.Execute(
                Expression.Call(
                    null,
                    GetMethodInfo(First, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    )
                ).First();
        }

        public static TSource FirstOrDefault<TSource>(this ITableQueryable<TSource> source) where TSource : class
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider.Execute(
                Expression.Call(
                    null,
                    GetMethodInfo(FirstOrDefault, source),
                    new Expression[] { source.Expression }
                    )
                ).FirstOrDefault();
        }

        public static TSource FirstOrDefault<TSource>(this ITableQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Provider.Execute(
                Expression.Call(
                    null,
                    GetMethodInfo(FirstOrDefault, source, predicate),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    )
                ).FirstOrDefault();
        }
    }
}