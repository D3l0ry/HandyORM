using System;

using DatabaseManager.Interfaces;
using DatabaseManager.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace DatabaseManager.TableInteractions
{
    internal class TableProviderExtensions : ITableProviderExtensions
    {
        private readonly SqlConnection mr_SqlConnection;
        private readonly TableQueryCreator mr_TableQueryCreator;
        private readonly ExpressionTranslator mr_TableQueryTranslator;
        private readonly TableConvertManager mr_TableConvertManager;

        public TableProviderExtensions(Type tableType, SqlConnection sqlConnection)
        {
            if (tableType == null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            mr_SqlConnection = sqlConnection;
            mr_TableQueryCreator = new TableQueryCreator(tableType);
            mr_TableQueryTranslator = new ExpressionTranslator(mr_TableQueryCreator);
            mr_TableConvertManager = new TableConvertManager(tableType, this);
        }

        public TableProviderExtensions(Type tableType, SqlConnection sqlConnection, TableQueryCreator tableQueryCreator)
        {
            if (tableType == null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            if (sqlConnection == null)
            {
                throw new ArgumentNullException(nameof(sqlConnection));
            }

            if (tableQueryCreator == null)
            {
                throw new ArgumentNullException(nameof(tableQueryCreator));
            }

            mr_SqlConnection = sqlConnection;
            mr_TableQueryCreator = tableQueryCreator;
            mr_TableQueryTranslator = new ExpressionTranslator(mr_TableQueryCreator);
            mr_TableConvertManager = new TableConvertManager(tableType, this);
        }

        public SqlConnection Connection => mr_SqlConnection;

        public TableQueryCreator Creator => mr_TableQueryCreator;

        public ExpressionTranslator Translator => mr_TableQueryTranslator;

        public TableConvertManager Converter => mr_TableConvertManager;
    }
}