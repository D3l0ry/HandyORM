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

            _MainTable = mainTable;
            _MainTableForeignKey = mainTableForeignKey;
            _SqlConnection = connection;
        }

        private string GetOrCreateForeignTableQuery()
        {
            Type genericType = typeof(Table);
            Type tableType = genericType.GetElementType() ?? genericType;

            if (_ForeignTableQuery.TryGetValue(tableType, out string foundForeingTableQuery))
            {
                return foundForeingTableQuery;
            }

            TableQueryCreator selectedTableQueryCreator = TableQueryCreator.GetInstance(tableType);
            TableProperties selectedTablePropertyQueryManager = selectedTableQueryCreator.Properties;
            StringBuilder queryString = new StringBuilder(selectedTableQueryCreator.MainQuery);

            if (!genericType.IsArray)
            {
                queryString.Insert(6, " TOP 1 ");
            }

            string primaryKeyName = selectedTablePropertyQueryManager
                .GetPropertyName(selectedTablePropertyQueryManager.PrimaryKey);

            queryString.Append($" WHERE ");
            queryString.Append(primaryKeyName);
            queryString.Append("=");

            string newForeingTableQuery = queryString.ToString();

            _ForeignTableQuery.Add(tableType, newForeingTableQuery);

            return newForeingTableQuery;
        }

        private string GetForeignTableQuery()
        {
            object mainTableForeignKeyValue = _MainTableForeignKey.GetValue(_MainTable);

            string selectedForeingTableQuery = GetOrCreateForeignTableQuery();
            StringBuilder stringBuilder = new StringBuilder(selectedForeingTableQuery);
            string foreignKeyValue = TableProperties.ConvertFieldQuery(mainTableForeignKeyValue);

            stringBuilder.Append(selectedForeingTableQuery);
            stringBuilder.Append(foreignKeyValue);
            stringBuilder.Append(';');

            return stringBuilder.ToString();
        }

        public Table Value
        {
            get
            {
                if (_Value == null)
                {
                    string newQuery = GetForeignTableQuery();
                    DbDataReader dataReader = _SqlConnection.ExecuteReader(newQuery);

                    _Value = _SqlConnection.ConvertReader<Table>(dataReader, null);
                }

                return _Value;
            }
        }
    }
}