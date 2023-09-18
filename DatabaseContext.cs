using System;
using System.Collections.Generic;
using System.Data.Common;

using Handy.Interfaces;

namespace Handy
{
    /// <summary>
    /// Абстрактный класс для работы с базой данных и ее таблицами
    /// </summary>
    public abstract class DatabaseContext : IDisposable
    {
        private readonly ContextOptions _options;
        private readonly Dictionary<Type, IDataQueryable> _tables;

        protected DatabaseContext()
        {
            ContextOptionsBuilder optionsBuilder = new ContextOptionsBuilder();

            OnConfigure(optionsBuilder);

            _options = optionsBuilder.Build();
            _tables = new Dictionary<Type, IDataQueryable>();

            _options.Connection.ConnectionString = _options.ConnectionString;
            _options.Connection.Open();
        }

        protected DatabaseContext(string connection)
        {
            ContextOptionsBuilder optionsBuilder = new ContextOptionsBuilder();
            optionsBuilder.UseConnectionString(connection);

            OnConfigure(optionsBuilder);

            _options = optionsBuilder.Build();
            _tables = new Dictionary<Type, IDataQueryable>();

            _options.Connection.ConnectionString = _options.ConnectionString;
            _options.Connection.Open();
        }

        public DbConnection Connection => _options.Connection;

        /// <summary>
        /// Вызывается при инициализации и до момента подключения к бд
        /// </summary>
        /// <param name="options"></param>
        protected abstract void OnConfigure(ContextOptionsBuilder options);

        protected virtual IEnumerable<T> ExecuteProcedure<T>(string procedure, params DbParameter[] arguments) where T : new() =>
            _options.Connection.ExecuteProcedure<T>(procedure, arguments);

        /// <summary>
        /// Получает объект TableManager с указанным типом, который определяет модель таблицы из базы данных
        /// </summary>
        /// <typeparam name="Table">Тип, определяющий модель таблицы из базы данных</typeparam>
        /// <returns></returns>
        protected Table<Table> GetTable<Table>() where Table : class, new()
        {
            Type tableType = typeof(Table);
            bool tryGet = _tables.TryGetValue(tableType, out IDataQueryable selectedTable);

            if (tryGet)
            {
                return (Table<Table>)selectedTable;
            }

            Table<Table> newTable = new Table<Table>(_options);

            _tables.Add(tableType, newTable);

            return newTable;
        }

        public void Dispose()
        {
            _tables.Clear();
            _options.Connection.Close();
        }
    }
}