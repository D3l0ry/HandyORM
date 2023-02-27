using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace Handy.Interfaces
{
    public interface IDataQueryProvider<T> where T : class, new()
    {
        IDataQueryable<T> CreateQuery(Expression expression);

        IEnumerable<T> Execute(Expression expression);
    }
}