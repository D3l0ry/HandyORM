using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq
{
    public interface ITableQueryProvider<TElement> where TElement : class
    {
        ITableQueryable<TElement> CreateQuery(Expression expression);

        IEnumerable<TElement> Execute(Expression expression);
    }
}