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
        private readonly TablePropertyInformation mr_PropertyInformation;
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

            mr_PropertyInformation = new TablePropertyInformation(tableType, mr_TableAttribute);
        }

        public TableAttribute Attribute => mr_TableAttribute;

        public TablePropertyInformation PropertyQueryCreator => mr_PropertyInformation;

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

        private void CreateLeftJoinForForeignColumn(Dictionary<string, string> foreignTablesQueryList, ColumnAttribute currentPropertyColumn, TableQueryCreator foreignTableQueryCreator)
        {
            if (foreignTablesQueryList.ContainsKey(foreignTableQueryCreator.Attribute.Name))
            {
                return;
            }

            StringBuilder newLeftJoinStringBuilder = new StringBuilder("LEFT JOIN ");

            string tableName = foreignTableQueryCreator.mr_PropertyInformation
                .GetTableName();

            string foreignTableKeyName;

            if (string.IsNullOrWhiteSpace(currentPropertyColumn.ForeignTableKeyName))
            {
                foreignTableKeyName = foreignTableQueryCreator.mr_PropertyInformation
                    .GetPropertyName(foreignTableQueryCreator.mr_PropertyInformation.PrimaryKey);
            }
            else
            {
                KeyValuePair<PropertyInfo, ColumnAttribute> selectedForeignTableProperty =
                    foreignTableQueryCreator.mr_PropertyInformation
                    .GetProperty(currentPropertyColumn.ForeignTableKeyName);

                foreignTableKeyName = foreignTableQueryCreator.mr_PropertyInformation
                    .GetPropertyName(selectedForeignTableProperty);
            }

            string foreignKeyName = mr_PropertyInformation
                .GetForeignKeyName(currentPropertyColumn);

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
            int propertiesCount = mr_PropertyInformation.Properties.Length;

            for (int index = 0; index < propertiesCount; index++)
            {
                KeyValuePair<PropertyInfo, ColumnAttribute> currentProperty = mr_PropertyInformation.GetProperty(index);
                ColumnAttribute currentPropertyColumn = currentProperty.Value;
                TablePropertyInformation propertyInformation = mr_PropertyInformation;

                if (currentPropertyColumn.IsTable)
                {
                    continue;
                }

                if (currentPropertyColumn.IsForeignColumn && !string.IsNullOrWhiteSpace(currentPropertyColumn.ForeignKeyName) && currentPropertyColumn.ForeignTable != null)
                {
                    TableQueryCreator foreignTableQueryCreator = GetInstance(currentPropertyColumn.ForeignTable);
                    propertyInformation = foreignTableQueryCreator.mr_PropertyInformation;

                    CreateLeftJoinForForeignColumn(foreignTablesQueryList, currentPropertyColumn, foreignTableQueryCreator);
                }

                string propertyName = propertyInformation.GetPropertyName(currentProperty);

                translatedQuery.Append(propertyName);

                if (index != propertiesCount - 1)
                {
                    translatedQuery.Append(", ");
                }
            }

            translatedQuery.Append(" FROM ");
            translatedQuery.Append(mr_PropertyInformation.GetTableName());
            translatedQuery.Append(" ");
            translatedQuery.AppendStringArray(foreignTablesQueryList.Values.ToArray());

            return translatedQuery.ToString();
        }

        internal static TableQueryCreator GetInstance(Type tableType)
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