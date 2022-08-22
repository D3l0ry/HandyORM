using System;
using System.Collections.Generic;
using DatabaseManager.DatabaseInteractions;
using Microsoft.Data.SqlClient;

namespace DatabaseManager
{
    public abstract class DatabaseContext : IDisposable
    {
        private readonly Dictionary<string, object> mr_Tables;
        private readonly Dictionary<string, ProcedureExecutor> mr_Procedures;

        internal readonly SqlConnection mr_SqlConnection;

        protected DatabaseContext(string connection)
        {
            if (string.IsNullOrWhiteSpace(connection))
            {
                throw new ArgumentNullException(nameof(connection));
            }

            mr_Tables = new Dictionary<string, object>();
            mr_Procedures = new Dictionary<string, ProcedureExecutor>();

            mr_SqlConnection = new SqlConnection(connection);

            mr_SqlConnection.Open();
        }

        public SqlConnection Connection => mr_SqlConnection;

        /// <summary>
        /// Метод для вызова процедур базы данных.
        /// Если процедура имеет принимаемые аргументы, то ExecuteProcedure должен обязательно вызываться в методе
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedure">Имя хранимой процедуры</param>
        /// <param name="arguments">Аргументы, которые передаются в процедуру. Аргументы должны идти в порядке параметров метода</param>
        /// <returns></returns>
        protected virtual T ExecuteProcedure<T>(string procedure, params object[] arguments)
        {
            ProcedureExecutor selectedProcedure;

            if (mr_Procedures.ContainsKey(procedure))
            {
                selectedProcedure = mr_Procedures[procedure];
            }
            else
            {
                selectedProcedure = new ProcedureExecutor(mr_SqlConnection, procedure);

                mr_Procedures.Add(procedure, selectedProcedure);
            }

            return selectedProcedure.Execute<T>(arguments);
        }

        protected TableManager<Table> GetTable<Table>()
        {
            Type tableType = typeof(Table);

            if (mr_Tables.TryGetValue(tableType.Name, out object selectedTable))
            {
                return selectedTable as TableManager<Table>;
            }

            TableManager<Table> newTable = new TableManager<Table>(mr_SqlConnection);

            mr_Tables.Add(tableType.Name, newTable);

            return newTable;
        }

        public void Dispose()
        {
            mr_Tables.Clear();
            mr_Procedures.Clear();
            mr_SqlConnection.Close();
        }
    }
}