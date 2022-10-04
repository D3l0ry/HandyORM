using System;
using System.Collections.Generic;
using System.Linq;

using Handy.DatabaseInteractions;

using Microsoft.Data.SqlClient;

namespace Handy
{
    /// <summary>
    /// Абстрактный класс для работы с базой данных и ее таблицами
    /// </summary>
    public abstract class DatabaseContext : IDisposable
    {
        private readonly SqlConnection mr_SqlConnection;
        private readonly Dictionary<Type, ITableQueryable> mr_Tables;

        protected DatabaseContext(string connection)
        {
            if (string.IsNullOrWhiteSpace(connection))
            {
                throw new ArgumentNullException(nameof(connection));
            }

            mr_Tables = new Dictionary<Type, ITableQueryable>();
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
        protected virtual T ExecuteProcedure<T>(string procedure, params object[] arguments) => mr_SqlConnection.ExecuteProcedure<T>(procedure, arguments);

        /// <summary>
        /// Получает объект TableManager с указанным типо, который определяет модель таблицы из базы данных
        /// </summary>
        /// <typeparam name="Table">Тип, определяющий модель таблицы из базы данных</typeparam>
        /// <returns></returns>
        protected TableManager<Table> GetTable<Table>() where Table : class, new()
        {
            Type tableType = typeof(Table);

            bool tryGet = mr_Tables.TryGetValue(tableType, out ITableQueryable selectedTable);

            if (tryGet)
            {
                return (TableManager<Table>)selectedTable;
            }

            TableManager<Table> newTable = new TableManager<Table>(mr_SqlConnection);

            mr_Tables.Add(tableType, newTable);

            return newTable;
        }

        public void Dispose()
        {
            mr_Tables.Clear();
            mr_SqlConnection.Close();
        }
    }
}