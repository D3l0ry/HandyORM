using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DatabaseManager.QueryInteractions
{
    internal class TableQueryCreator
    {
        private readonly TableQueryProvider mr_TableQueryProvider;

        private readonly TableAttribute mr_TableAttribute;

        private readonly Dictionary<string, TableQueryProvider> mr_NestedTableQueries;

        private readonly TablePropertyQueryManager mr_PropertyQueryCreator;

        private readonly StringBuilder mr_MainQuery;

        public TableQueryCreator(Type tableType, TableQueryProvider queryProvider)
        {
            if (tableType is null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            mr_TableAttribute = tableType.GetCustomAttribute<TableAttribute>();

            if (mr_TableAttribute is null)
            {
                throw new NullReferenceException($"Таблица не объявлена с атрибутом {nameof(TableAttribute)}");
            }

            mr_TableQueryProvider = queryProvider;

            mr_PropertyQueryCreator = new TablePropertyQueryManager(tableType, mr_TableAttribute);

            mr_NestedTableQueries = new Dictionary<string, TableQueryProvider>();

            mr_MainQuery = CreateInitialTableQuery();
        }

        public TableAttribute Attribute => mr_TableAttribute;

        public TablePropertyQueryManager PropertyQueryCreator => mr_PropertyQueryCreator;

        public Dictionary<string, TableQueryProvider> ForeignTables => mr_NestedTableQueries;

        public string MainQuery => mr_MainQuery.ToString();

        /// <summary>
        /// Создание запроса для главной таблицы
        /// </summary>
        /// <returns></returns>
        private StringBuilder CreateInitialTableQuery()
        {
            StringBuilder translatedQuery = new StringBuilder("SELECT ");
            Dictionary<string, string> foreignTablesQueryList = new Dictionary<string, string>();

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in mr_PropertyQueryCreator.Properties)
            {
                ColumnAttribute columnAttribute = currentKeyValuePair.Value;

                if (columnAttribute.IsTable || columnAttribute.IsArray)
                {
                    continue;
                }

                if (columnAttribute.IsForeignColumn && !string.IsNullOrWhiteSpace(columnAttribute.ForeignKeyName) && columnAttribute.ForeignTable != null)
                {
                    TableQueryCreator foreignTableQueryCreator = null;

                    if (ForeignTables.TryGetValue(columnAttribute.ForeignTable.Name, out TableQueryProvider tableQueryProvider))
                    {
                        foreignTableQueryCreator = tableQueryProvider.Creator;
                    }
                    else
                    {
                        TableQueryProvider foreignTableQueryProvider = new TableQueryProvider(columnAttribute.ForeignTable, mr_TableQueryProvider.Connection);

                        ForeignTables.Add(columnAttribute.ForeignTable.Name, foreignTableQueryProvider);

                        foreignTableQueryCreator = foreignTableQueryProvider.Creator;
                    }

                    if (!foreignTablesQueryList.ContainsKey(foreignTableQueryCreator.Attribute.Name))
                    {
                        foreignTablesQueryList
                            .Add(foreignTableQueryCreator.Attribute.Name,
                            $"LEFT JOIN {foreignTableQueryCreator.mr_PropertyQueryCreator.GetTableName()} ON " +
                            $"{foreignTableQueryCreator.mr_PropertyQueryCreator.GetPropertyName(foreignTableQueryCreator.mr_PropertyQueryCreator.PrimaryKey)}=" +
                            $"{mr_PropertyQueryCreator.GetForeignKeyName(currentKeyValuePair)} ");
                    }

                    translatedQuery.Append($"{foreignTableQueryCreator.mr_PropertyQueryCreator.GetPropertyName(currentKeyValuePair)}, ");

                    continue;
                }

                translatedQuery.Append($"{mr_PropertyQueryCreator.GetPropertyName(currentKeyValuePair)}, ");
            }

            translatedQuery.Remove(translatedQuery.Length - 2, 1);
            translatedQuery.Append($" FROM {mr_PropertyQueryCreator.GetTableName()} ");

            translatedQuery.Append(foreignTablesQueryList.Values.SelectMany(currentQuery => currentQuery).ToArray());

            return translatedQuery;
        }

        public static string CreateForeignTableQuery(TableQueryCreator foreignTableQueryManager, ColumnAttribute propertyAttribute)
        {
            StringBuilder queryString = new StringBuilder(foreignTableQueryManager.MainQuery);

            if (propertyAttribute.IsTable)
            {
                queryString.Insert(6, " TOP 1 ");
            }

            queryString.Append($" WHERE ");

            if (string.IsNullOrWhiteSpace(propertyAttribute.ForeignTableKey))
            {
                queryString
                    .Append($"{foreignTableQueryManager.mr_PropertyQueryCreator.GetPropertyName(foreignTableQueryManager.mr_PropertyQueryCreator.PrimaryKey)}=");
            }
            else
            {
                KeyValuePair<PropertyInfo, ColumnAttribute> selectedForeingTableProperty = foreignTableQueryManager.mr_PropertyQueryCreator
                    .GetProperty(propertyAttribute.ForeignTableKey);

                queryString
                    .Append($"{foreignTableQueryManager.mr_PropertyQueryCreator.GetPropertyName(selectedForeingTableProperty)}=");
            }

            return queryString.ToString();
        }
    }
}