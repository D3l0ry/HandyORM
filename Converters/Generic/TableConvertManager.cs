using System.Collections.Generic;

using Microsoft.Data.SqlClient;

namespace Handy.Converters.Generic
{
    internal class TableConvertManager<Table> : TableConvertManager where Table : class, new()
    {
        internal TableConvertManager(SqlConnection connection) : base(typeof(Table), connection) { }

        public new Table GetObject(SqlDataReader dataReader) => (Table)base.GetObject(dataReader);

        public new Table[] GetObjects(SqlDataReader dataReader) => (Table[])base.GetObjects(dataReader);

        public new IEnumerable<Table> GetObjectsEnumerable(SqlDataReader dataReader)
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