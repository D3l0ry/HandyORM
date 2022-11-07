using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Handy.Interfaces;
using Handy.TableInteractions;

using Microsoft.Data.SqlClient;

namespace Handy
{
    internal class TableQueryProvider<TElement> : ITableQueryProvider<TElement>, ITableQueryHelper where TElement : class, new()
    {
        private readonly SqlConnection mr_SqlConnection;
        private readonly ITableProviderExtensions mr_ProviderExtensions;

        internal TableQueryProvider(SqlConnection sqlConnection)
        {
            mr_SqlConnection = sqlConnection;
            mr_ProviderExtensions = new TableProviderExtensions(typeof(TElement), sqlConnection);
        }

        public SqlConnection Connection => mr_SqlConnection;

        public ITableProviderExtensions Extensions => mr_ProviderExtensions;

        ITableQueryable<TElement> ITableQueryProvider<TElement>.CreateQuery(Expression expression) => new TableManager<TElement>(this, expression, mr_SqlConnection);

        public IEnumerable<TElement> Execute(Expression expression)
        {
            StringBuilder mainQuery = new StringBuilder(mr_ProviderExtensions.Creator.MainQuery);

            string query = mr_ProviderExtensions.Translator.Translate(expression);

            mainQuery.Append(query);

            SqlDataReader dataReader = mr_SqlConnection.ExecuteReader(mainQuery.ToString());

            IEnumerable<TElement> result = mr_SqlConnection
                .GetTableConverter<TElement>()
                .GetObjectsEnumerable(dataReader);

            return result;
        }
    }
}