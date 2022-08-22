using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseManager.QueryInteractions;
using Microsoft.Data.SqlClient;

namespace DatabaseManager
{
    internal class TableQueryProvider : IDatabaseQueryProvider
    {
        private readonly SqlConnection mr_SqlConnection;
        private readonly TableQueryCreator mr_TableQueryCreator;
        private readonly ExpressionTranslator mr_TableQueryTranslator;
        private readonly TableConvertManager mr_TableConvertManager;

        internal TableQueryProvider(Type tableType, SqlConnection sqlConnection)
        {
            if (tableType is null)
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

        private SqlDataReader GetDataReader(string query)
        {
            SqlCommand sqlCommand = mr_SqlConnection.CreateCommand();

            sqlCommand.CommandText = query;

            SqlDataReader dataReader = sqlCommand.ExecuteReader();

            sqlCommand.Dispose();

            return dataReader;
        }

        private TResult Convert<TResult>(SqlDataReader dataReader)
        {
            Type resultType = typeof(TResult);

            if (resultType.IsArray)
            {
                return (TResult)mr_TableConvertManager.GetObjects(dataReader);
            }

            return (TResult)mr_TableConvertManager.GetObject(dataReader);
        }

        IDatabaseQueryable<TElement> IDatabaseQueryProvider.CreateQuery<TElement>(Expression expression) =>
            new TableManager<TElement>(this, expression, mr_SqlConnection);

        public TResult Execute<TResult>(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            SqlDataReader dataReader = GetDataReader(query);

            return Convert<TResult>(dataReader);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            string query = Translator.Translate(expression);

            SqlDataReader dataReader = GetDataReader(query);

            dataReader.CheckDataValue(expression);

            return Convert<TResult>(dataReader);
        }
    }
}