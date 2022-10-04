using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Handy.InternalInteractions;

namespace Handy.QueryInteractions
{
    internal class TableQueryCreator
    {
        private readonly TableAttribute mr_TableAttribute;
        private readonly TablePropertyQueryManager mr_PropertyQueryCreator;
        private string mr_MainQuery;

        public TableQueryCreator(Type tableType)
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

        private void CreateLeftJoinForForeignColumn(Dictionary<string, string> foreignTablesQueryList, KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair, TableQueryCreator foreignTableQueryCreator)
        {
            if (foreignTablesQueryList.ContainsKey(foreignTableQueryCreator.Attribute.Name))
            {
                return;
            }

            StringBuilder newLeftJoinStringBuilder = new StringBuilder("LEFT JOIN ");

            string tableName = foreignTableQueryCreator.mr_PropertyQueryCreator
                .GetTableName();

            string PropertyPrimaryKeyName = foreignTableQueryCreator.mr_PropertyQueryCreator
                .GetPropertyName(foreignTableQueryCreator.mr_PropertyQueryCreator.PrimaryKey);

            string foreignKeyName = mr_PropertyQueryCreator
                .GetForeignKeyName(currentKeyValuePair);

            newLeftJoinStringBuilder.Append(tableName);
            newLeftJoinStringBuilder.Append(" ON ");
            newLeftJoinStringBuilder.Append(PropertyPrimaryKeyName);
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

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in mr_PropertyQueryCreator.Properties)
            {
                ColumnAttribute columnAttribute = currentKeyValuePair.Value;

                if (columnAttribute.IsTable)
                {
                    continue;
                }

                if (columnAttribute.IsForeignColumn && !string.IsNullOrWhiteSpace(columnAttribute.ForeignKeyName) && columnAttribute.ForeignTable != null)
                {
                    TableQueryCreator foreignTableQueryCreator = InternalStaticArrays.GetOrCreateTableQueryCreator(columnAttribute.ForeignTable);

                    CreateLeftJoinForForeignColumn(foreignTablesQueryList, currentKeyValuePair, foreignTableQueryCreator);

                    translatedQuery.Append(foreignTableQueryCreator.mr_PropertyQueryCreator.GetPropertyName(currentKeyValuePair));
                    translatedQuery.Append(", ");

                    continue;
                }

                translatedQuery.Append(mr_PropertyQueryCreator.GetPropertyName(currentKeyValuePair));
                translatedQuery.Append(", ");
            }

            translatedQuery.Remove(translatedQuery.Length - 2, 1);
            translatedQuery.Append(" FROM ");
            translatedQuery.Append(mr_PropertyQueryCreator.GetTableName());
            translatedQuery.Append(" ");

            char[] foreignTablesLeftJoinQuery = foreignTablesQueryList.Values
                .SelectMany(currentQuery => currentQuery)
                .ToArray();

            translatedQuery.Append(foreignTablesLeftJoinQuery);

            return translatedQuery.ToString();
        }
    }
}