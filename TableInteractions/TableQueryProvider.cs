using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Handy.Interfaces;
using Handy.QueryInteractions;
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
            Type tableType = typeof(TElement);

            mr_SqlConnection = sqlConnection;
            mr_ProviderExtensions = new TableProviderExtensions(tableType, sqlConnection);
        }

        public ITableProviderExtensions Extensions => mr_ProviderExtensions;

        ITableQueryable<TElement> ITableQueryProvider<TElement>.CreateQuery(Expression expression) => new TableManager<TElement>(this, expression, mr_SqlConnection);

        public IEnumerable<TElement> Execute(Expression expression)
        {
            string query = mr_ProviderExtensions.Translator.Translate(expression);

            SqlDataReader dataReader = mr_SqlConnection.ExecuteReader(query);

            IEnumerable result = mr_ProviderExtensions.Converter.GetObjectsEnumerable(dataReader);

            foreach (TElement currentElement in result)
            {
                yield return currentElement;
            }
        }
    }
}