using System.Collections.Generic;
using System.Linq.Expressions;

using Handy.Interfaces;

namespace System.Linq
{
    internal interface ITableQueryHelper
    {
        ITableProviderExtensions Extensions { get; }
    }

    public interface ITableQueryProvider<TElement> where TElement : class
    {
        ITableQueryable<TElement> CreateQuery(Expression expression);

        IEnumerable<TElement> Execute(Expression expression);
    }
}