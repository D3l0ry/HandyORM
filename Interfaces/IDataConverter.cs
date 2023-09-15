using System.Collections.Generic;
using System.Data.Common;

namespace Handy.Interfaces
{
    internal interface IDataConverter<T> where T : new()
    {
        IEnumerable<T> Query(DbDataReader dataReader);

        T GetObject(DbDataReader dataReader);

        T[] GetObjects(DbDataReader dataReader);
    }
}