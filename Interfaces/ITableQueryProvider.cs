using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

namespace System.Linq
{
    public interface ITableQueryProvider<TElement> where TElement : class
    {
        DbConnection Connection { get; }

        ITableQueryable<TElement> CreateQuery(Expression expression);

        IEnumerable<TElement> Execute(Expression expression);
    }
}