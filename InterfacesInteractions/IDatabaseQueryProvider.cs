using System.Linq.Expressions;

namespace System.Linq
{
    public interface IDatabaseQueryProvider
    {
        IDatabaseQueryable<TElement> CreateQuery<TElement>(Expression expression);

        TResult Execute<TResult>(Expression expression);
    }
}