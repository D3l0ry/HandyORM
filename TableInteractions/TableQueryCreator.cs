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
        private static readonly Dictionary<Type, TableQueryCreator> _TableQueryCreators = new Dictionary<Type, TableQueryCreator>();

        private readonly Type _TableType;
        private readonly TableAttribute _TableAttribute;
        private readonly TableProperties _PropertyInformation;
        private string _MainQuery;

        private TableQueryCreator(Type tableType)
        {
            if (tableType == null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            _TableType = tableType;
            _TableAttribute = tableType.GetCustomAttribute<TableAttribute>();

            if (_TableAttribute == null)
            {
                throw new NullReferenceException($"Таблица не объявлена с атрибутом {nameof(TableAttribute)}");
            }

            _PropertyInformation = new TableProperties(tableType, _TableAttribute);
        }

        public TableAttribute Attribute => _TableAttribute;

        public TableProperties PropertyQueryCreator => _PropertyInformation;

        public string MainQuery
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_MainQuery))
                {
                    _MainQuery = CreateInitialTableQuery();
                }

                return _MainQuery;
            }
        }

        private TableQueryCreator AddLeftJoinForForeignColumn(Dictionary<string, string> foreignTablesQueryList, in KeyValuePair<PropertyInfo, ColumnAttribute> currentProperty)
        {
            ColumnAttribute currentPropertyColumn = currentProperty.Value;

            if (string.IsNullOrWhiteSpace(currentPropertyColumn.ForeignKeyName))
            {
                throw new NullReferenceException($"Не указан внешний ключ для поля {currentProperty.Key.Name} в классе {_TableType.Name}");
            }

            if (currentPropertyColumn.ForeignTable == null)
            {
                throw new NullReferenceException($"Не указан тип внешней таблицы поля {currentProperty.Key.Name} в классе {_TableType.Name}");
            }

            TableQueryCreator foreignTableQueryCreator = GetInstance(currentPropertyColumn.ForeignTable);

            if (foreignTablesQueryList.ContainsKey(foreignTableQueryCreator.Attribute.Name))
            {
                return foreignTableQueryCreator;
            }

            StringBuilder newLeftJoinStringBuilder = new StringBuilder("LEFT JOIN ");

            string tableName = foreignTableQueryCreator._PropertyInformation
                .GetTableName();

            string foreignTableKeyName;

            if (string.IsNullOrWhiteSpace(currentPropertyColumn.ForeignTableKeyName))
            {
                foreignTableKeyName = foreignTableQueryCreator._PropertyInformation
                    .GetPropertyName(foreignTableQueryCreator._PropertyInformation.PrimaryKey);
            }
            else
            {
                KeyValuePair<PropertyInfo, ColumnAttribute> selectedForeignTableProperty =
                    foreignTableQueryCreator._PropertyInformation
                    .GetProperty(currentPropertyColumn.ForeignTableKeyName);

                foreignTableKeyName = foreignTableQueryCreator._PropertyInformation
                    .GetPropertyName(selectedForeignTableProperty);
            }

            string foreignKeyName = _PropertyInformation
                .GetForeignKeyName(currentPropertyColumn);

            newLeftJoinStringBuilder.Append(tableName);
            newLeftJoinStringBuilder.Append(" ON ");
            newLeftJoinStringBuilder.Append(foreignTableKeyName);
            newLeftJoinStringBuilder.Append("=");
            newLeftJoinStringBuilder.Append(foreignKeyName);
            newLeftJoinStringBuilder.Append(" ");

            string newLeftJoinString = newLeftJoinStringBuilder.ToString();

            foreignTablesQueryList.Add(foreignTableQueryCreator.Attribute.Name, newLeftJoinString);

            return foreignTableQueryCreator;
        }

        /// <summary>
        /// Создание запроса для главной таблицы
        /// </summary>
        /// <returns></returns>
        private string CreateInitialTableQuery()
        {
            StringBuilder translatedQuery = new StringBuilder("SELECT ");
            Dictionary<string, string> foreignTablesQueryList = new Dictionary<string, string>();

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentProperty in _PropertyInformation)
            {
                ColumnAttribute currentPropertyColumn = currentProperty.Value;
                TableProperties propertyInformation = _PropertyInformation;

                if (currentPropertyColumn.IsTable)
                {
                    continue;
                }

                if (currentPropertyColumn.IsForeignColumn)
                {
                    TableQueryCreator foreignTableQueryCreator = AddLeftJoinForForeignColumn(foreignTablesQueryList, currentProperty);
                    propertyInformation = foreignTableQueryCreator._PropertyInformation;
                }

                string propertyName = propertyInformation.GetPropertyName(currentProperty);

                translatedQuery.Append(propertyName);
                translatedQuery.Append(",");
            }

            translatedQuery.Remove(translatedQuery.Length - 1, 1);
            translatedQuery.Append(" FROM ");
            translatedQuery.Append(_PropertyInformation.GetTableName());
            translatedQuery.Append(" ");
            translatedQuery.AppendStringArray(foreignTablesQueryList.Values.ToArray());

            return translatedQuery.ToString();
        }

        internal static TableQueryCreator GetInstance(Type tableType)
        {
            if (_TableQueryCreators.TryGetValue(tableType, out TableQueryCreator foundTableQueryCreator))
            {
                return foundTableQueryCreator;
            }

            TableQueryCreator newTableQueryCreator = new TableQueryCreator(tableType);

            _TableQueryCreators.Add(tableType, newTableQueryCreator);

            return newTableQueryCreator;
        }
    }
}