using System;
using System.Linq;
using System.Reflection;
using System.Text;
using DatabaseManager.QueryInteractions;
using DatabaseManager.TableInteractions;
using Microsoft.Data.SqlClient;

namespace DatabaseManager
{
    public class ForeignTable<Table> where Table : class
    {
        private readonly object mr_MainTable;
        private readonly PropertyInfo mr_MainTableForeignKey;
        private readonly TableProviderExtensions mr_TableQueryProvider;
        private readonly ColumnAttribute mr_ColumnAttribute;

        private Table m_Value;

        internal ForeignTable(object mainTable, PropertyInfo mainTableForeignKey, TableProviderExtensions queryProvider, ColumnAttribute propertyAttribute)
        {
            if (mainTable is null)
            {
                throw new ArgumentNullException(nameof(mainTable));
            }

            if (mainTableForeignKey is null)
            {
                throw new ArgumentNullException(nameof(mainTableForeignKey));
            }

            if (queryProvider is null)
            {
                throw new ArgumentNullException(nameof(queryProvider));
            }

            if (propertyAttribute is null)
            {
                throw new ArgumentNullException(nameof(propertyAttribute));
            }

            mr_MainTable = mainTable;
            mr_MainTableForeignKey = mainTableForeignKey;
            mr_TableQueryProvider = queryProvider;
            mr_ColumnAttribute = propertyAttribute;
        }

        private string GetForeignTableQuery()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(TableQueryCreator.CreateForeignTableQuery(mr_TableQueryProvider.Creator, mr_ColumnAttribute));
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

                SqlDataReader dataReader = mr_TableQueryProvider.Connection.ExecuteReader(newQuery);

                m_Value = (Table)mr_TableQueryProvider.Converter.GetObject(dataReader);

                return m_Value;
            }
        }

        public override string ToString() => Value?.ToString() ?? "";
    }
}