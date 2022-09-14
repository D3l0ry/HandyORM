using System.Linq.Expressions;

using DatabaseManager.Interfaces;

namespace System.Linq
{
    internal interface IDatabaseQueryHelper
    {
        ITableProviderExtensions Extensions { get; }
    }

    public interface IDatabaseQueryProvider
    {
        TResult Execute<TResult>(Expression expression) where TResult : class;
    }

    public interface IDatabaseQueryProvider<TElement> : IDatabaseQueryProvider
    {
        IDatabaseQueryable<TElement> CreateQuery(Expression expression);
    }
}