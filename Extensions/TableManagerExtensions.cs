using System.Collections.Generic;

using Handy.Converters.Generic;

using Microsoft.Data.SqlClient;

namespace Handy.Extensions
{
    internal static class TableManagerExtensions
    {
        public static IEnumerable<Table> Query<Table>(this TableManager<Table> tableManager, string query) where Table : class, new()
        {
            SqlConnection connection = tableManager.Provider.Connection;

            TableConvertManager<Table> tableConvertManager = connection.GetTableConverter<Table>();

            SqlDataReader dataReader = connection.ExecuteReader(query);

            IEnumerable<Table> value = tableConvertManager.GetObjectsEnumerable(dataReader);

            return value;
        }
    }
}