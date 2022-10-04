using System;
using System.Diagnostics;
using System.Reflection;

using Handy.Interfaces;
using Handy.QueryInteractions;
using Handy.TableInteractions;

using Microsoft.Data.SqlClient;

namespace Handy.DatabaseInteractions
{
    internal static class ProcedureExecutor
    {
        private static void AddArguments(object[] arguments, SqlCommand dataCommand, StackFrame stackFrame, MethodBase callingMethod)
        {
            if (arguments.Length == 0)
            {
                return;
            }

            if (!stackFrame.HasMethod())
            {
                return;
            }

            ParameterInfo[] methodParameters = callingMethod.GetParameters();

            for (int index = 0; index < methodParameters.Length; index++)
            {
                ParameterInfo currentParameter = methodParameters[index];
                ParameterAttribute parameterAttribute = methodParameters[index].GetCustomAttribute<ParameterAttribute>();

                string parameterName = parameterAttribute is null ? $"@{currentParameter.Name}" : $"@{parameterAttribute.Name}";

                SqlParameter newParameter = new SqlParameter()
                {
                    ParameterName = parameterName,
                    Value = arguments[index]
                };

                dataCommand.Parameters.Add(newParameter);
            }
        }

        private static bool IsDatabaseTableType(Type type)
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

        private static ITableProviderExtensions GetTableProviderExtensions(Type type, SqlConnection sqlConnection) => new TableProviderExtensions(type, sqlConnection);

        private static TResult ConvertReader<TResult>(SqlConnection sqlConnection, SqlDataReader dataReader)
        {
            if (dataReader == null)
            {
                throw new ArgumentNullException(nameof(dataReader));
            }

            Type resultType = typeof(TResult);
            TResult result;

            if (IsDatabaseTableType(resultType))
            {
                ITableProviderExtensions tableQueryProvider;

                if (resultType.IsArray)
                {
                    tableQueryProvider = GetTableProviderExtensions(resultType.GetElementType(), sqlConnection);

                    result = (TResult)tableQueryProvider.Converter.GetObjects(dataReader);
                }
                else
                {
                    tableQueryProvider = GetTableProviderExtensions(resultType, sqlConnection);

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

        /// <summary>
        /// Метод для вызова процедур базы данных.
        /// Если процедура имеет принимаемые аргументы, то ExecuteProcedure должен обязательно вызываться в методе
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlConnection"></param>
        /// <param name="procedureName"></param>
        /// <param name="arguments">Аргументы, которые передаются в процедуру. Аргументы должны идти в порядке параметров метода</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T ExecuteProcedure<T>(this SqlConnection sqlConnection, string procedureName, params object[] arguments)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentNullException(nameof(procedureName));
            }

            SqlCommand dataCommand = sqlConnection.CreateProcedureCommand(procedureName);

            StackFrame stackFrame = new StackFrame(2);
            MethodBase callingMethod = stackFrame.GetMethod();

            AddArguments(arguments, dataCommand, stackFrame, callingMethod);

            SqlDataReader dataReader = dataCommand.ExecuteReader();

            T result = ConvertReader<T>(sqlConnection, dataReader);

            dataCommand.Dispose();
            dataReader.Close();

            return result;
        }
    }
}