using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

using Handy.QueryInteractions;

namespace Handy
{
    public class ForeignTable<Table> where Table : class, new()
    {
        private static readonly Dictionary<Type, string> ms_ForeignTableQuery = new Dictionary<Type, string>();

        private readonly Type mr_TableType;
        private readonly object mr_MainTable;
        private readonly PropertyInfo mr_MainTableForeignKey;
        private readonly DbConnection mr_SqlConnection;

        private Table m_Value;

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

            mr_TableType = typeof(Table);
            mr_MainTable = mainTable;
            mr_MainTableForeignKey = mainTableForeignKey;
            mr_SqlConnection = connection;
        }

        private string GetOrCreateForeignTableQuery()
        {
            if (ms_ForeignTableQuery.TryGetValue(mr_TableType, out string foundForeingTableQuery))
            {
                return foundForeingTableQuery;
            }

            TableQueryCreator selectedTableQueryCreator = TableQueryCreator.GetInstance(mr_TableType);
            TablePropertyInformation selectedTablePropertyQueryManager = selectedTableQueryCreator.PropertyQueryCreator;
            StringBuilder queryString = new StringBuilder(selectedTableQueryCreator.MainQuery);

            if (!mr_TableType.IsArray)
            {
                queryString.Insert(6, " TOP 1 ");
            }

            string primaryKeyName = selectedTablePropertyQueryManager
                .GetPropertyName(selectedTablePropertyQueryManager.PrimaryKey);

            queryString.Append($" WHERE ");
            queryString.Append(primaryKeyName);
            queryString.Append("=");

            string newForeingTableQuery = queryString.ToString();

            ms_ForeignTableQuery.Add(mr_TableType, newForeingTableQuery);

            return newForeingTableQuery;
        }

        private string GetForeignTableQuery()
        {
            string selectedForeingTableQuery = GetOrCreateForeignTableQuery();
            StringBuilder stringBuilder = new StringBuilder(selectedForeingTableQuery);
            string foreignKeyValue = TablePropertyInformation.ConvertFieldQuery(mr_MainTableForeignKey.GetValue(mr_MainTable));

            stringBuilder.Append(selectedForeingTableQuery);
            stringBuilder.Append(foreignKeyValue);
            stringBuilder.Append(';');

            return stringBuilder.ToString();
        }

        public Table Value
        {
            get
            {
                if (m_Value != null)
                {
                    return m_Value;
                }

                string newQuery = GetForeignTableQuery();
                DbDataReader dataReader = mr_SqlConnection.ExecuteReader(newQuery);

                m_Value = mr_SqlConnection.ConvertReader<Table>(dataReader);

                return m_Value;
            }
        }
    }
}