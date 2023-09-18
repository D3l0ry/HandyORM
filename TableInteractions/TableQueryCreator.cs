using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Handy.Extensions;

namespace Handy.TableInteractions
{
    public class TableQueryCreator
    {
        private static readonly Dictionary<Type, TableQueryCreator> _tableQueryCreators = new Dictionary<Type, TableQueryCreator>();

        private readonly Type _tableType;
        private readonly TableAttribute _tableAttribute;
        private readonly TableProperties _propertyInformation;
        private string _mainQuery;

        private TableQueryCreator(Type tableType)
        {
            _tableType = tableType ?? throw new ArgumentNullException(nameof(tableType));
            _tableAttribute = tableType.GetCustomAttribute<TableAttribute>();

            if (_tableAttribute == null)
            {
                throw new NullReferenceException($"Таблица не объявлена с атрибутом {nameof(TableAttribute)}");
            }

            _propertyInformation = new TableProperties(tableType, _tableAttribute);
        }

        public TableAttribute Attribute => _tableAttribute;

        public TableProperties Properties => _propertyInformation;

        public string MainQuery
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_mainQuery))
                {
                    _mainQuery = CreateInitialTableQuery();
                }

                return _mainQuery;
            }
        }

        /// <summary>
        /// Создание запроса для главной таблицы
        /// </summary>
        /// <returns></returns>
        private string CreateInitialTableQuery()
        {
            StringBuilder translatedQuery = new StringBuilder("SELECT ");
            Dictionary<string, string> foreignTablesQueryList = new Dictionary<string, string>();

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentProperty in _propertyInformation)
            {
                string propertyName = _propertyInformation.GetPropertyName(currentProperty);

                translatedQuery.Append(propertyName);
                translatedQuery.Append(",");
            }

            translatedQuery.Remove(translatedQuery.Length - 1, 1);
            translatedQuery.Append(" FROM ");
            translatedQuery.Append(_tableAttribute.GetFullTableName());
            translatedQuery.Append(" ");
            translatedQuery.AppendStringArray(foreignTablesQueryList.Values.ToArray());

            return translatedQuery.ToString();
        }

        internal static TableQueryCreator GetInstance(Type tableType)
        {
            if (_tableQueryCreators.TryGetValue(tableType, out TableQueryCreator foundTableQueryCreator))
            {
                return foundTableQueryCreator;
            }

            TableQueryCreator newTableQueryCreator = new TableQueryCreator(tableType);

            _tableQueryCreators.Add(tableType, newTableQueryCreator);

            return newTableQueryCreator;
        }
    }
}