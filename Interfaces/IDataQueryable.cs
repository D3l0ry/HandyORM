using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace Handy.Interfaces
{
    public interface IDataQueryable
    {
        Type ElementType { get; }

        Expression Expression { get; }
    }

    public interface IDataQueryable<T> : IDataQueryable, IEnumerable<T> where T : class, new()
    {
        IDataQueryProvider<T> Provider { get; }
    }
}