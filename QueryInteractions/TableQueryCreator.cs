using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Handy.Extensions;

namespace Handy.QueryInteractions
{
    public class TableQueryCreator
    {
        private static readonly Dictionary<Type, TableQueryCreator> ms_TableQueryCreators = new Dictionary<Type, TableQueryCreator>();

        private readonly TableAttribute mr_TableAttribute;
        private readonly TablePropertyQueryManager mr_PropertyQueryCreator;
        private string mr_MainQuery;

        public TableQueryCreator(Type tableType)
        {
            if (tableType == null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            mr_TableAttribute = tableType.GetCustomAttribute<TableAttribute>();

            if (mr_TableAttribute == null)
            {
                throw new NullReferenceException($"Таблица не объявлена с атрибутом {nameof(TableAttribute)}");
            }

            mr_PropertyQueryCreator = new TablePropertyQueryManager(tableType, mr_TableAttribute);
        }

        public TableAttribute Attribute => mr_TableAttribute;

        public TablePropertyQueryManager PropertyQueryCreator => mr_PropertyQueryCreator;

        public string MainQuery
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mr_MainQuery))
                {
                    mr_MainQuery = CreateInitialTableQuery();
                }

                return mr_MainQuery;
            }
        }

        private void CreateLeftJoinForForeignColumn(Dictionary<string, string> foreignTablesQueryList, KeyValuePair<PropertyInfo, ColumnAttribute> currentProperty, TableQueryCreator foreignTableQueryCreator)
        {
            if (foreignTablesQueryList.ContainsKey(foreignTableQueryCreator.Attribute.Name))
            {
                return;
            }

            StringBuilder newLeftJoinStringBuilder = new StringBuilder("LEFT JOIN ");

            string tableName = foreignTableQueryCreator.mr_PropertyQueryCreator
                .GetTableName();

            string foreignTableKeyName;

            if (string.IsNullOrWhiteSpace(currentProperty.Value.ForeignTableKeyName))
            {
                foreignTableKeyName = foreignTableQueryCreator.mr_PropertyQueryCreator
                    .GetPropertyName(foreignTableQueryCreator.mr_PropertyQueryCreator.PrimaryKey);
            }
            else
            {
                KeyValuePair<PropertyInfo, ColumnAttribute> selectedForeignTableProperty =
                    foreignTableQueryCreator.mr_PropertyQueryCreator
                    .GetProperty(currentProperty.Value.ForeignTableKeyName);

                foreignTableKeyName = foreignTableQueryCreator.mr_PropertyQueryCreator
                    .GetPropertyName(selectedForeignTableProperty);
            }

            string foreignKeyName = mr_PropertyQueryCreator
                .GetForeignKeyName(currentProperty);

            newLeftJoinStringBuilder.Append(tableName);
            newLeftJoinStringBuilder.Append(" ON ");
            newLeftJoinStringBuilder.Append(foreignTableKeyName);
            newLeftJoinStringBuilder.Append("=");
            newLeftJoinStringBuilder.Append(foreignKeyName);
            newLeftJoinStringBuilder.Append(" ");

            string newLeftJoinString = newLeftJoinStringBuilder.ToString();

            foreignTablesQueryList.Add(foreignTableQueryCreator.Attribute.Name, newLeftJoinString);
        }

        /// <summary>
        /// Создание запроса для главной таблицы
        /// </summary>
        /// <returns></returns>
        private string CreateInitialTableQuery()
        {
            StringBuilder translatedQuery = new StringBuilder("SELECT ");
            Dictionary<string, string> foreignTablesQueryList = new Dictionary<string, string>();
            int propertiesCount = mr_PropertyQueryCreator.Properties.Length;

            for (int index = 0; index < propertiesCount; index++)
            {
                KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair = mr_PropertyQueryCreator.Properties[index];
                ColumnAttribute columnAttribute = currentKeyValuePair.Value;

                if (columnAttribute.IsTable)
                {
                    continue;
                }

                if (columnAttribute.IsForeignColumn && !string.IsNullOrWhiteSpace(columnAttribute.ForeignKeyName) && columnAttribute.ForeignTable != null)
                {
                    TableQueryCreator foreignTableQueryCreator = GetOrCreateTableQueryCreator(columnAttribute.ForeignTable);

                    CreateLeftJoinForForeignColumn(foreignTablesQueryList, currentKeyValuePair, foreignTableQueryCreator);

                    translatedQuery.Append(foreignTableQueryCreator.mr_PropertyQueryCreator.GetPropertyName(currentKeyValuePair));
                    translatedQuery.Append(", ");

                    continue;
                }

                translatedQuery.Append(mr_PropertyQueryCreator.GetPropertyName(currentKeyValuePair));

                if (index != propertiesCount - 1)
                {
                    translatedQuery.Append(", ");
                }
            }

            translatedQuery.Append(" FROM ");
            translatedQuery.Append(mr_PropertyQueryCreator.GetTableName());
            translatedQuery.Append(" ");
            translatedQuery.AppendStringArray(foreignTablesQueryList.Values.ToArray());

            return translatedQuery.ToString();
        }

        public static TableQueryCreator GetOrCreateTableQueryCreator(Type tableType)
        {
            if (ms_TableQueryCreators.TryGetValue(tableType, out TableQueryCreator foundTableQueryCreator))
            {
                return foundTableQueryCreator;
            }

            TableQueryCreator newTableQueryCreator = new TableQueryCreator(tableType);

            ms_TableQueryCreators.Add(tableType, newTableQueryCreator);

            return newTableQueryCreator;
        }
    }
}