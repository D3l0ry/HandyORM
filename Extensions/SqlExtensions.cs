using System;
using System.Collections.Generic;
using System.Linq;

using Handy.Converters;

using Microsoft.Data.SqlClient;

namespace Handy
{
    public static class SqlExtensions
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
            GenericConvertManager<T> convertManager = new GenericConvertManager<T>();

            IEnumerable<T> objectsEnumerable = convertManager.GetObjectsEnumerable(dataReader);

            return objectsEnumerable;
        }

        public static T First<T>(this SqlConnection connection, string query) where T : new()
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
            GenericConvertManager<T> convertManager = new GenericConvertManager<T>();

            IEnumerable<T> objectsEnumerable = convertManager.GetObjectsEnumerable(dataReader);

            T firstElement = objectsEnumerable.First();

            return firstElement;
        }

        public static T FirstOrDefault<T>(this SqlConnection connection, string query) where T : new()
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
            GenericConvertManager<T> convertManager = new GenericConvertManager<T>();

            IEnumerable<T> objectsEnumerable = convertManager.GetObjectsEnumerable(dataReader);

            T firstElement = objectsEnumerable.FirstOrDefault();

            return firstElement;
        }
    }
}