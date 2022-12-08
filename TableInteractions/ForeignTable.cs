using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

using Handy.TableInteractions;

namespace Handy
{
    public class ForeignTable<Table> where Table : class, new()
    {
        private static readonly Dictionary<Type, string> _ForeignTableQuery = new Dictionary<Type, string>();

        private readonly Type _TableType;
        private readonly object _MainTable;
        private readonly PropertyInfo _MainTableForeignKey;
        private readonly DbConnection _SqlConnection;

        private Table _Value;

        internal ForeignTable(object mainTable, PropertyInfo mainTableForeignKey, DbConnection connection)
        {
            if (mainTable is null)
            {
                throw new ArgumentNullException(nameof(mainTable));
            }

            if (mainTableForeignKey is null)
            {
                throw new ArgumentNullException(nameof(mainTableForeignKey));
            }

            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _TableType = typeof(Table);
            _MainTable = mainTable;
            _MainTableForeignKey = mainTableForeignKey;
            _SqlConnection = connection;
        }

        private string GetOrCreateForeignTableQuery()
        {
            if (_ForeignTableQuery.TryGetValue(_TableType, out string foundForeingTableQuery))
            {
                return foundForeingTableQuery;
            }

            TableQueryCreator selectedTableQueryCreator = TableQueryCreator.GetInstance(_TableType);
            TableProperties selectedTablePropertyQueryManager = selectedTableQueryCreator.PropertyQueryCreator;
            StringBuilder queryString = new StringBuilder(selectedTableQueryCreator.MainQuery);

            if (!_TableType.IsArray)
            {
                queryString.Insert(6, " TOP 1 ");
            }

            string primaryKeyName = selectedTablePropertyQueryManager
                .GetPropertyName(selectedTablePropertyQueryManager.PrimaryKey);

            queryString.Append($" WHERE ");
            queryString.Append(primaryKeyName);
            queryString.Append("=");

            string newForeingTableQuery = queryString.ToString();

            _ForeignTableQuery.Add(_TableType, newForeingTableQuery);

            return newForeingTableQuery;
        }

        private string GetForeignTableQuery()
        {
            string selectedForeingTableQuery = GetOrCreateForeignTableQuery();
            StringBuilder stringBuilder = new StringBuilder(selectedForeingTableQuery);
            string foreignKeyValue = TableProperties.ConvertFieldQuery(_MainTableForeignKey.GetValue(_MainTable));

            stringBuilder.Append(selectedForeingTableQuery);
            stringBuilder.Append(foreignKeyValue);
            stringBuilder.Append(';');

            return stringBuilder.ToString();
        }

        public Table Value
        {
            get
            {
                if (_Value != null)
                {
                    return _Value;
                }

                string newQuery = GetForeignTableQuery();
                DbDataReader dataReader = _SqlConnection.ExecuteReader(newQuery);

                _Value = _SqlConnection.ConvertReader<Table>(dataReader);

                return _Value;
            }
        }
    }
}