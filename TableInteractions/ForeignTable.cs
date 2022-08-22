using System;
using System.Reflection;
using System.Text;
using DatabaseManager.QueryInteractions;

namespace DatabaseManager
{
    public class ForeignTable<Table> where Table : class
    {
        private readonly object mr_MainTable;
        private readonly PropertyInfo mr_MainTableForeignKey;
        private readonly TableQueryProvider mr_TableQueryProvider;
        private readonly ColumnAttribute mr_ColumnAttribute;

        private Table m_Value;

        internal ForeignTable(object mainTable, PropertyInfo mainTableForeignKey, TableQueryProvider queryProvider, ColumnAttribute propertyAttribute)
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

        public Table Value
        {
            get
            {
                if (m_Value != null)
                {
                    return m_Value;
                }

                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(TableQueryCreator.CreateForeignTableQuery(mr_TableQueryProvider.Creator, mr_ColumnAttribute));
                stringBuilder.Append(TablePropertyQueryManager.ConvertFieldQuery(mr_MainTableForeignKey.GetValue(mr_MainTable)));
                stringBuilder.Append(";");

                string newQuery = stringBuilder.ToString();

                m_Value = mr_TableQueryProvider.Execute<Table>(newQuery);

                return m_Value;
            }
        }

        public override string ToString() => Value?.ToString() ?? "";
    }
}