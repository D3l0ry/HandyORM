using System;

using Handy.Interfaces;
using Handy.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace Handy.TableInteractions
{
    internal class TableProviderExtensions : ITableProviderExtensions
    {
        private readonly SqlConnection mr_SqlConnection;
        private readonly TableQueryCreator mr_TableQueryCreator;
        private readonly ExpressionTranslator mr_TableQueryTranslator;

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
            mr_TableQueryCreator = TableQueryCreator.GetOrCreateTableQueryCreator(tableType);
            mr_TableQueryTranslator = new ExpressionTranslator(mr_TableQueryCreator);
        }

        public SqlConnection Connection => mr_SqlConnection;

        public TableQueryCreator Creator => mr_TableQueryCreator;

        public ExpressionTranslator Translator => mr_TableQueryTranslator;
    }
}