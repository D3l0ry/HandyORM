using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
using Handy.Interfaces;

namespace Handy
{
    public static class DataQueryable
    {
        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1) => f.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2) => f.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3) => f.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4) => f.Method;

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6)
        {
            return f.Method;
        }

        public static IDataQueryable<TSource> Where<TSource>(this IDataQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Provider
                .CreateQuery(Expression
                    .Call(null, GetMethodInfo(Where, source, predicate), new Expression[] { source.Expression, Expression.Quote(predicate) }));
        }

        public static TSource First<TSource>(this IDataQueryable<TSource> source) where TSource : class, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider
                .Execute(Expression
                    .Call(null, GetMethodInfo(First, source), new Expression[] { source.Expression }))
                .First();
        }

        public static TSource First<TSource>(this IDataQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Provider
                .Execute(Expression
                    .Call(null, GetMethodInfo(First, source, predicate), new Expression[] { source.Expression, Expression.Quote(predicate) }))
                .First();
        }

        public static TSource FirstOrDefault<TSource>(this IDataQueryable<TSource> source) where TSource : class, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Provider
                .Execute(Expression
                    .Call(null, GetMethodInfo(FirstOrDefault, source), new Expression[] { source.Expression }))
                .FirstOrDefault();
        }

        public static TSource FirstOrDefault<TSource>(this IDataQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) where TSource : class, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Provider
                .Execute(Expression
                    .Call(null, GetMethodInfo(FirstOrDefault, source, predicate), new Expression[] { source.Expression, Expression.Quote(predicate) }))
                .FirstOrDefault();
        }
    }
}