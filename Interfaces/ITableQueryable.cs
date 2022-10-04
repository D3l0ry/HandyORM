using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq
{
    public interface ITableQueryable : IEnumerable
    {
        Expression Expression { get; }
    }

    public interface ITableQueryable<TElement> : ITableQueryable, IEnumerable<TElement>, IEnumerable where TElement : class
    {
        ITableQueryProvider<TElement> Provider { get; }
    }
}