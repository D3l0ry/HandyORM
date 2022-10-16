using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Handy.Interfaces;
using Handy.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace Handy
{
    public class ForeignTable<Table> where Table : class, new()
    {
        private static readonly Dictionary<Type, string> ms_ForeignTableQuery = new Dictionary<Type, string>();

        private readonly object mr_MainTable;
        private readonly PropertyInfo mr_MainTableForeignKey;
        private readonly ITableProviderExtensions mr_TableProviderExtensions;

        private Table m_Value;

        internal ForeignTable(object mainTable, PropertyInfo mainTableForeignKey, ITableProviderExtensions tableProvider)
        {
            if (mainTable is null)
            {
                throw new ArgumentNullException(nameof(mainTable));
            }

            if (mainTableForeignKey is null)
            {
                throw new ArgumentNullException(nameof(mainTableForeignKey));
            }

            if (tableProvider is null)
            {
                throw new ArgumentNullException(nameof(tableProvider));
            }

            mr_MainTable = mainTable;
            mr_MainTableForeignKey = mainTableForeignKey;
            mr_TableProviderExtensions = tableProvider;
        }

        private static string GetOrCreateForeignTableQuery(ITableProviderExtensions tableProviderExtensions)
        {
            if (ms_ForeignTableQuery.TryGetValue(tableProviderExtensions.TableType, out string foundForeingTableQuery))
            {
                return foundForeingTableQuery;
            }

            TableQueryCreator selectedTableQueryCreator = tableProviderExtensions.Creator;
            TablePropertyQueryManager selectedTablePropertyQueryManager = selectedTableQueryCreator.PropertyQueryCreator;

            StringBuilder queryString = new StringBuilder(selectedTableQueryCreator.MainQuery);

            queryString.Insert(6, " TOP 1 ");
            queryString.Append($" WHERE ");
            queryString.Append(selectedTablePropertyQueryManager.GetPropertyName(selectedTablePropertyQueryManager.PrimaryKey));
            queryString.Append("=");

            string newForeingTableQuery = queryString.ToString();

            ms_ForeignTableQuery.Add(tableProviderExtensions.TableType, newForeingTableQuery);

            return newForeingTableQuery;
        }

        private string GetForeignTableQuery()
        {
            StringBuilder stringBuilder = new StringBuilder();

            string selectedForeingTableQuery = GetOrCreateForeignTableQuery(mr_TableProviderExtensions);

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

                SqlDataReader dataReader = mr_TableProviderExtensions.Connection.ExecuteReader(newQuery);

                m_Value = (Table)mr_TableProviderExtensions.Converter.GetObject(dataReader);

                return m_Value;
            }
        }
    }
}