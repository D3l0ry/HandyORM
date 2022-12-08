using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;

using Handy.Extensions;

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
            DataConverter convertManager = new DataConverter(typeof(T));

            IEnumerable<T> objectsEnumerable = (IEnumerable<T>)convertManager.GetObjects(dataReader);

            return objectsEnumerable;
        }

        public static void ExecuteNonQuery(this DbConnection sqlConnection, string query)
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

                sqlCommand.ExecuteNonQuery();
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
        public static T ExecuteProcedure<T>(this DbConnection sqlConnection, string procedureName, params object[] arguments)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentNullException(nameof(procedureName));
            }

            DbCommand dataCommand = sqlConnection.CreateProcedureCommand(procedureName);
            StackFrame stackFrame = new StackFrame(2);
            MethodBase callingMethod = stackFrame.GetMethod();

            dataCommand.AddArguments(arguments, stackFrame, callingMethod);

            DbDataReader dataReader = dataCommand.ExecuteReader();

            T result = sqlConnection.ConvertReader<T>(dataReader);

            dataCommand.Dispose();
            dataReader.Close();

            return result;
        }
    }
}