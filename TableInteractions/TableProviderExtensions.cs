using System;
using System.Collections.Generic;
using System.Text;

using DatabaseManager.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace DatabaseManager.TableInteractions
{
    internal class TableProviderExtensions
    {
        private readonly SqlConnection mr_SqlConnection;
        private readonly TableQueryCreator mr_TableQueryCreator;
        private readonly ExpressionTranslator mr_TableQueryTranslator;
        private readonly TableConvertManager mr_TableConvertManager;

        public TableProviderExtensions(Type tableType, SqlConnection sqlConnection)
        {
            if(tableType == null)
            {
                throw new ArgumentNullException(nameof(tableType));
            }

            mr_SqlConnection = sqlConnection;
            mr_TableQueryCreator = new TableQueryCreator(tableType, this);
            mr_TableQueryTranslator = new ExpressionTranslator(this);
            mr_TableConvertManager = new TableConvertManager(tableType, this);
        }

        public SqlConnection Connection => mr_SqlConnection;

        public TableQueryCreator Creator => mr_TableQueryCreator;

        public ExpressionTranslator Translator => mr_TableQueryTranslator;

        public TableConvertManager Converter => mr_TableConvertManager;
    }
}