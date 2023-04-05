using System;
using System.Data.Common;

namespace Handy
{
    internal static class SqlConnectionExtensions
    {
        public static DbCommand CreateCommand(this DbConnection connection, string query)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            DbCommand sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = query;

            return sqlCommand;
        }

        public static DbDataReader ExecuteReader(this DbConnection connection, string query)
        {
            DbCommand command = connection.CreateCommand(query);

            return command.ExecuteReader();
        }
    }
}