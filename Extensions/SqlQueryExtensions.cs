using System;
using System.Collections.Generic;
using System.Linq;

using Handy.Converters.Generic;

using Microsoft.Data.SqlClient;

namespace Handy
{
    public static class SqlQueryExtensions
    {
        public static IEnumerable<T> Query<T>(this SqlConnection connection, string query) where T : new()
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            SqlDataReader dataReader = connection.ExecuteReader(query);
            ConvertManager<T> convertManager = new ConvertManager<T>();

            IEnumerable<T> objectsEnumerable = convertManager.GetObjectsEnumerable(dataReader);

            return objectsEnumerable;
        }

        public static T First<T>(this SqlConnection connection, string query) where T : new() => connection.Query<T>(query).First();

        public static T FirstOrDefault<T>(this SqlConnection connection, string query) where T : new() => connection.Query<T>(query).FirstOrDefault();
    }
}