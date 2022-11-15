using System.Collections.Generic;
using System.Data.Common;

namespace Handy.Converters.Generic
{
    internal class TableConvertManager<Table> : TableConvertManager where Table : class, new()
    {
        internal TableConvertManager(DbConnection connection) : base(typeof(Table), connection) { }

        public new IEnumerable<Table> GetObjectsEnumerable(DbDataReader dataReader)
        {
            using (dataReader)
            {
                while (dataReader.Read())
                {
                    yield return (Table)GetInternalObject(dataReader);
                }
            }
        }
    }
}