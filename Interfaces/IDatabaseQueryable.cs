using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq
{
    public interface IDatabaseQueryable : IEnumerable
    {
        Expression Expression { get; }

        IDatabaseQueryProvider Provider { get; }
    }

    public interface IDatabaseQueryable<out TElement> : IEnumerable<TElement>, IEnumerable, IDatabaseQueryable { }
}