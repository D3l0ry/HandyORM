using System;
using System.Linq;
using System.Linq.Expressions;

using DatabaseManager.Interfaces;
using DatabaseManager.TableInteractions;

using Microsoft.Data.SqlClient;

namespace DatabaseManager
{
    internal class TableQueryProvider<TElement> : IDatabaseQueryProvider<TElement>, IDatabaseQueryHelper where TElement : class, new()
    {
        private readonly SqlConnection mr_SqlConnection;
        private readonly TableProviderExtensions mr_ProviderExtensions;

        internal TableQueryProvider(SqlConnection sqlConnection)
        {
            Type tableType = typeof(TElement);

            mr_SqlConnection = sqlConnection;
            mr_ProviderExtensions = new TableProviderExtensions(tableType, sqlConnection);
        }

        public ITableProviderExtensions Extensions => mr_ProviderExtensions;

        private SqlDataReader GetDataReader(string query)
        {
            SqlCommand sqlCommand = mr_SqlConnection.CreateCommand();

            sqlCommand.CommandText = query;

            SqlDataReader dataReader = sqlCommand.ExecuteReader();

            sqlCommand.Dispose();

            return dataReader;
        }

        private TResult Convert<TResult>(SqlDataReader dataReader) where TResult : class
        {
            Type resultType = typeof(TResult);
            TResult result;

            if (resultType.IsArray)
            {
                result = (TResult)mr_ProviderExtensions.Converter.GetObjects(dataReader);
            }
            else
            {
                result = (TResult)mr_ProviderExtensions.Converter.GetObject(dataReader);
            }

            dataReader.Close();

            return result;
        }

        IDatabaseQueryable<TElement> IDatabaseQueryProvider<TElement>.CreateQuery(Expression expression) => new TableManager<TElement>(this, expression, mr_SqlConnection);

        public TResult Execute<TResult>(Expression expression) where TResult : class
        {
            string query = mr_ProviderExtensions.Translator.Translate(expression);

            SqlDataReader dataReader = GetDataReader(query);

            dataReader.CheckDataValue(expression);

            return Convert<TResult>(dataReader);
        }
    }
}