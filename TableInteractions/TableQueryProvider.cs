using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Handy.Interfaces;
using Handy.QueryInteractions;

namespace Handy
{
    internal class TableQueryProvider<TElement> : ITableQueryProvider<TElement> where TElement : class, new()
    {
        private readonly IExpressionTranslatorBuilder mr_ExpressionTranslatorBuilder;
        private readonly DbConnection mr_SqlConnection;
        private readonly TableQueryCreator mr_TableQueryCreator;

        internal TableQueryProvider(IExpressionTranslatorBuilder expressionTranslatorBuilder,DbConnection sqlConnection)
        {
            Type tableType = typeof(TElement);

            mr_ExpressionTranslatorBuilder = expressionTranslatorBuilder;
            mr_SqlConnection = sqlConnection;
            mr_TableQueryCreator = TableQueryCreator.GetOrCreateTableQueryCreator(tableType);
        }

        public DbConnection Connection => mr_SqlConnection;

        public TableQueryCreator Creator => mr_TableQueryCreator;

        internal IExpressionTranslatorBuilder ExpressionTranslatorBuilder => mr_ExpressionTranslatorBuilder;

        ITableQueryable<TElement> ITableQueryProvider<TElement>.CreateQuery(Expression expression) => new TableManager<TElement>(this, expression,mr_ExpressionTranslatorBuilder, mr_SqlConnection);

        public IEnumerable<TElement> Execute(Expression expression)
        {
            StringBuilder mainQuery = new StringBuilder(mr_TableQueryCreator.MainQuery);

            string query = mr_ExpressionTranslatorBuilder
                .CreateInstance(mr_TableQueryCreator)
                .ToString(expression);

            mainQuery.Append(query);

            DbDataReader dataReader = mr_SqlConnection.ExecuteReader(mainQuery.ToString());

            IEnumerable<TElement> result = mr_SqlConnection
                .GetTableConverter<TElement>()
                .GetObjectsEnumerable(dataReader);

            return result;
        }
    }
}