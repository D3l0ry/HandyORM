using System;

using Microsoft.Data.SqlClient;

namespace DatabaseManager.QueryInteractions
{
    internal static class QueryExtensions
    {
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