using System.Collections.Generic;
using System.Linq.Expressions;

using Handy.Interfaces;

namespace System.Linq
{
    internal interface ITableQueryHelper
    {
        ITableProviderExtensions Extensions { get; }
    }

    public interface ITableQueryProvider { }

    public interface ITableQueryProvider<TElement> : ITableQueryProvider where TElement : class
    {
        ITableQueryable<TElement> CreateQuery(Expression expression);

        IEnumerable<TElement> Execute(Expression expression);
    }
}