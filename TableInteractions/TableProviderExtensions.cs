using System;

using Handy.Interfaces;
using Handy.InternalInteractions;
using Handy.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace Handy.TableInteractions
{
    internal class TableProviderExtensions : ITableProviderExtensions
    {
        private readonly Type mr_TableType;
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

            mr_TableType = tableType;
            mr_SqlConnection = sqlConnection;
            mr_TableQueryCreator = InternalStaticArrays.GetOrCreateTableQueryCreator(tableType);
            mr_TableQueryTranslator = new ExpressionTranslator(mr_TableQueryCreator);
            mr_TableConvertManager = new TableConvertManager(tableType, this);
        }

        public Type TableType => mr_TableType;

        public SqlConnection Connection => mr_SqlConnection;

        public TableQueryCreator Creator => mr_TableQueryCreator;

        public ExpressionTranslator Translator => mr_TableQueryTranslator;

        public TableConvertManager Converter => mr_TableConvertManager;
    }
}