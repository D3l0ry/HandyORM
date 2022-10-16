using System;
using System.Data;

using Microsoft.Data.SqlClient;

namespace Handy
{
    internal static class QueryExtensions
    {
        public static SqlCommand CreateProcedureCommand(this SqlConnection sqlConnection, string procedureName)
        {
            SqlCommand dataCommand = sqlConnection.CreateCommand();

            dataCommand.CommandType = CommandType.StoredProcedure;
            dataCommand.CommandText = procedureName;

            return dataCommand;
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

        public static SqlDataReader ExecuteReader(this SqlConnection sqlConnection, string query)
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

                return sqlCommand.ExecuteReader();
            }
        }
    }
}