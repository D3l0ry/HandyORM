using System;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace DatabaseManager.QueryInteractions
{
    internal static class QueryExtensions
    {
        private static bool IsDatabaseTableType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type elementType = type;

            if (type.IsArray)
            {
                elementType = type.GetElementType();
            }

            return elementType.GetCustomAttribute<TableAttribute>() != null;
        }

        public static void ExecuteNonQuery(this SqlConnection sqlConnection, string query)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = query;

                sqlCommand.ExecuteNonQuery();
            }
        }

        public static TResult ConvertReader<TResult>(this SqlConnection sqlConnection, SqlDataReader dataReader)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (dataReader == null)
            {
                throw new ArgumentNullException(nameof(dataReader));
            }

            Type resultType = typeof(TResult);
            TResult result;

            if (resultType.IsDatabaseTableType())
            {
                TableQueryProvider tableQueryProvider;

                if (resultType.IsArray)
                {
                    tableQueryProvider = new TableQueryProvider(resultType.GetElementType(), sqlConnection);

                    result = (TResult)tableQueryProvider.Converter.GetObjects(dataReader);
                }
                else
                {
                    tableQueryProvider = new TableQueryProvider(resultType, sqlConnection);

                    result = (TResult)tableQueryProvider.Converter.GetObject(dataReader);
                }
            }
            else
            {
                ConvertManager convertManager;

                if (resultType.IsArray)
                {
                    convertManager = new ConvertManager(resultType.GetElementType());

                    result = (TResult)convertManager.GetObjects(dataReader);
                }
                else
                {
                    convertManager = new ConvertManager(resultType);

                    result = (TResult)convertManager.GetObject(dataReader);
                }
            }

            dataReader.Close();

            return result;
        }
    }
}