using System;
using System.Data;
using System.Data.Common;
using System.Reflection;

using Handy.Converter;

namespace Handy
{
    internal static class SqlConnectionExtensions
    {
        public static TResult ConvertReader<TResult>(this DbConnection sqlConnection, DbDataReader dataReader)
        {
            if (dataReader == null)
            {
                throw new ArgumentNullException(nameof(dataReader));
            }

            Type resultType = typeof(TResult);
            Type elementType = resultType.GetElementType() ?? resultType;
            TableAttribute tableAttribute = elementType.GetCustomAttribute<TableAttribute>();

            DataConverter convertManager;

            if (tableAttribute != null)
            {
                convertManager = sqlConnection.GetTableConverter(elementType);
            }
            else
            {
                convertManager = new DataConverter(elementType);
            }

            if (resultType.IsArray)
            {
                return (TResult)convertManager.GetObjects(dataReader);
            }

            return (TResult)convertManager.GetObject(dataReader);
        }

        public static DbCommand CreateProcedureCommand(this DbConnection sqlConnection, string procedureName)
        {
            DbCommand dataCommand = sqlConnection.CreateCommand();

            dataCommand.CommandType = CommandType.StoredProcedure;
            dataCommand.CommandText = procedureName;

            return dataCommand;
        }

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

        public static TableConverter GetTableConverter<Table>(this DbConnection connection) => connection.GetTableConverter(typeof(Table));

        public static TableConverter GetTableConverter(this DbConnection connection, Type tableType)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (tableType == null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            return new TableConverter(tableType, connection);
        }
    }
}