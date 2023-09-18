using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Handy.Converter;
using Handy.TableInteractions;

namespace Handy
{
    public class ForeignTable<Table> where Table : class, new()
    {
        private static readonly Dictionary<Type, string> _foreignTableQuery = new Dictionary<Type, string>();

        private readonly object _mainTable;
        private readonly PropertyInfo _mainTableForeignKey;
        private readonly DbConnection _sqlConnection;
        private Table[] _value;

        internal ForeignTable(object mainTable, PropertyInfo mainTableForeignKey, DbConnection connection)
        {
            _mainTable = mainTable ?? throw new ArgumentNullException(nameof(mainTable));
            _mainTableForeignKey = mainTableForeignKey ?? throw new ArgumentNullException(nameof(mainTableForeignKey));
            _sqlConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        private string GetOrCreateForeignTableQuery()
        {
            Type genericType = typeof(Table);
            Type tableType = genericType.GetElementType() ?? genericType;

            if (_foreignTableQuery.TryGetValue(tableType, out string foundForeingTableQuery))
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

            _foreignTableQuery.Add(tableType, newForeingTableQuery);

            return newForeingTableQuery;
        }

        private string GetForeignTableQuery()
        {
            object mainTableForeignKeyValue = _mainTableForeignKey.GetValue(_mainTable);

            string selectedForeingTableQuery = GetOrCreateForeignTableQuery();
            StringBuilder stringBuilder = new StringBuilder(selectedForeingTableQuery);
            string foreignKeyValue = TableProperties.ConvertFieldQuery(mainTableForeignKeyValue);

            stringBuilder.Append(selectedForeingTableQuery);
            stringBuilder.Append(foreignKeyValue);
            stringBuilder.Append(';');

            return stringBuilder.ToString();
        }

        public IEnumerable<Table> Value
        {
            get
            {
                if (_value == null)
                {
                    string newQuery = GetForeignTableQuery();
                    DbDataReader dataReader = _sqlConnection.ExecuteReader(newQuery);
                    TableConverter<Table> converter = new TableConverter<Table>(_sqlConnection);

                    _value = converter.Query(dataReader).ToArray();
                }

                return _value;
            }
        }
    }
}