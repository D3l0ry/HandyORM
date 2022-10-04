using System;
using System.Reflection;
using System.Text;

using Handy.Interfaces;
using Handy.InternalInteractions;
using Handy.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace Handy
{
    public class ForeignTable<Table> where Table : class, new()
    {
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

        private string GetForeignTableQuery()
        {
            StringBuilder stringBuilder = new StringBuilder();

            string selectedForeingTableQuery = InternalStaticArrays.GetOrCreateForeignTableQuery(mr_TableProviderExtensions);

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