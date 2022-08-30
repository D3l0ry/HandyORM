using System.Linq.Expressions;

using DatabaseManager.QueryInteractions;
using DatabaseManager;
using Microsoft.Data.SqlClient;
using DatabaseManager.TableInteractions;

namespace System.Linq
{
    internal interface IDatabaseQueryHelper
    {
        TableProviderExtensions Extensions { get; }
    }

    public interface IDatabaseQueryProvider
    {
        TResult Execute<TResult>(Expression expression);
    }

    public interface IDatabaseQueryProvider<TElement> : IDatabaseQueryProvider
    {
        IDatabaseQueryable<TElement> CreateQuery(Expression expression);
    }
}