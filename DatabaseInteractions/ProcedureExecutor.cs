using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using DatabaseManager.QueryInteractions;
using Microsoft.Data.SqlClient;

namespace DatabaseManager.DatabaseInteractions
{
    internal class ProcedureExecutor
    {
        private readonly SqlConnection mr_SqlConnection;
        private readonly string mr_ProcedureName;

        public ProcedureExecutor(SqlConnection sqlConnection, string procedureName)
        {
            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentNullException(nameof(procedureName));
            }

            mr_SqlConnection = sqlConnection;
            mr_ProcedureName = procedureName;
        }

        private SqlCommand CreateCommand()
        {
            SqlCommand dataCommand = mr_SqlConnection.CreateCommand();

            dataCommand.CommandType = CommandType.StoredProcedure;
            dataCommand.CommandText = mr_ProcedureName;

            return dataCommand;
        }

        private void AddArguments(object[] arguments, SqlCommand dataCommand, StackFrame stackFrame, MethodBase callingMethod)
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

        /// <summary>
        /// Метод для вызова процедур базы данных.
        /// Если процедура имеет принимаемые аргументы, то ExecuteProcedure должен обязательно вызываться в методе
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedure">Имя хранимой процедуры</param>
        /// <param name="arguments">Аргументы, которые передаются в процедуру. Аргументы должны идти в порядке параметров метода</param>
        /// <returns></returns>
        public T Execute<T>(params object[] arguments)
        {
            SqlCommand dataCommand = CreateCommand();

            StackFrame stackFrame = new StackFrame(2);
            MethodBase callingMethod = stackFrame.GetMethod();

            AddArguments(arguments, dataCommand, stackFrame, callingMethod);

            SqlDataReader dataReader = dataCommand.ExecuteReader();

            T result = mr_SqlConnection.ConvertReader<T>(dataReader);

            dataCommand.Dispose();
            dataReader.Close();

            return result;
        }
    }
}