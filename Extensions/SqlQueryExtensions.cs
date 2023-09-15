using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;

using Handy.Converter;
using Handy.Interfaces;

namespace Handy
{
    public static class SqlQueryExtensions
    {
        public static IEnumerable<T> Query<T>(this DbConnection connection, string query) where T : new()
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            DbDataReader dataReader = connection.ExecuteReader(query);
            DataConverter<T> convertManager = new DataConverter<T>();

            IEnumerable<T> objectsEnumerable = convertManager.GetObjects(dataReader);

            return objectsEnumerable;
        }

        public static int ExecuteNonQuery(this DbConnection sqlConnection, string query)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (DbCommand sqlCommand = sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = query;

                return sqlCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Метод для вызова процедуры из базы данных.
        /// Если процедура имеет принимаемые аргументы, то ExecuteProcedure должен обязательно вызываться в методе, который полностью копирует аргументы процедуры
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlConnection"></param>
        /// <param name="procedureName"></param>
        /// <param name="arguments">Аргументы, которые передаются в процедуру. Аргументы должны идти в порядке параметров метода</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<T> ExecuteProcedure<T>(this DbConnection connection, string procedureName, params DbParameter[] arguments) where T : new()
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentNullException(nameof(procedureName));
            }

            Type type = typeof(T);
            bool isTable = type.IsDefined(typeof(TableAttribute));

            DbCommand dataCommand = connection.CreateCommand();
            dataCommand.CommandType = CommandType.StoredProcedure;
            dataCommand.CommandText = procedureName;

            if (arguments != null && arguments.Length > 0)
            {
                dataCommand.Parameters.AddRange(arguments);
            }

            DbDataReader dataReader = dataCommand.ExecuteReader();
            IDataConverter<T> converter;

            if (isTable)
            {
                converter = new TableConverter<T>(connection);
            }
            else
            {
                converter = new DataConverter<T>();
            }

            return converter.Query(dataReader);
        }
    }
}