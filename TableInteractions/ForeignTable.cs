using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Handy.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace Handy
{
    public class ForeignTable<Table> where Table : class, new()
    {
        private static readonly Dictionary<Type, string> ms_ForeignTableQuery = new Dictionary<Type, string>();

        private readonly Type mr_TableType;
        private readonly object mr_MainTable;
        private readonly PropertyInfo mr_MainTableForeignKey;
        private readonly SqlConnection mr_SqlConnection;

        private Table m_Value;

        internal ForeignTable(object mainTable, PropertyInfo mainTableForeignKey, SqlConnection connection)
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

            TableQueryCreator selectedTableQueryCreator = TableQueryCreator.GetOrCreateTableQueryCreator(mr_TableType);
            TablePropertyQueryManager selectedTablePropertyQueryManager = selectedTableQueryCreator.PropertyQueryCreator;

            StringBuilder queryString = new StringBuilder(selectedTableQueryCreator.MainQuery);

            queryString.Insert(6, " TOP 1 ");
            queryString.Append($" WHERE ");
            queryString.Append(selectedTablePropertyQueryManager.GetPropertyName(selectedTablePropertyQueryManager.PrimaryKey));
            queryString.Append("=");

            string newForeingTableQuery = queryString.ToString();

            ms_ForeignTableQuery.Add(mr_TableType, newForeingTableQuery);

            return newForeingTableQuery;
        }

        private string GetForeignTableQuery()
        {
            StringBuilder stringBuilder = new StringBuilder();

            string selectedForeingTableQuery = GetOrCreateForeignTableQuery();

            stringBuilder.Append(selectedForeingTableQuery);
            stringBuilder.Append(TablePropertyQueryManager.ConvertFieldQuery(mr_MainTableForeignKey.GetValue(mr_MainTable)));
            stringBuilder.Append(";");

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

                SqlDataReader dataReader = mr_SqlConnection.ExecuteReader(newQuery);

                m_Value = mr_SqlConnection
                    .GetTableConverter<Table>()
                    .GetObject(dataReader);

                return m_Value;
            }
        }
    }
}