using System;
using System.Data.Common;
using System.Reflection;

using Handy.Converter;

namespace Handy
{
    internal static class SqlConnectionExtensions
    {
        public static DbDataReader ExecuteReader(this DbConnection sqlConnection, string query)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            DbCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = query;

            return sqlCommand.ExecuteReader();
        }
    }
}