using System.Collections.Generic;
using System.Linq.Expressions;

using Handy.Interfaces;

using Microsoft.Data.SqlClient;

namespace System.Linq
{
    internal interface ITableQueryHelper
    {
        ITableProviderExtensions Extensions { get; }
    }

    public interface ITableQueryProvider<TElement> where TElement : class
    {
        SqlConnection Connection { get; }

        ITableQueryable<TElement> CreateQuery(Expression expression);

        IEnumerable<TElement> Execute(Expression expression);
    }
}