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
        private readonly ContextOptions mr_ContextOptions;
        private readonly TableQueryCreator mr_TableQueryCreator;

        internal TableQueryProvider(ContextOptions options)
        {
            Type tableType = typeof(TElement);

            mr_ContextOptions = options;
            mr_TableQueryCreator = TableQueryCreator.GetInstance(tableType);
        }

        public DbConnection Connection => mr_ContextOptions.Connection;

        public IExpressionTranslatorBuilder ExpressionTranslatorBuilder => mr_ContextOptions.ExpressionTranslatorBuilder;

        public TableQueryCreator Creator => mr_TableQueryCreator;

        ITableQueryable<TElement> ITableQueryProvider<TElement>.CreateQuery(Expression expression) => new TableManager<TElement>(this, expression);

        public IEnumerable<TElement> Execute(Expression expression)
        {
            DbConnection connection = mr_ContextOptions.Connection;
            StringBuilder mainQuery = new StringBuilder(mr_TableQueryCreator.MainQuery);
            IExpressionTranslatorBuilder translatorBuilder = mr_ContextOptions.ExpressionTranslatorBuilder;

            string query = translatorBuilder
                .CreateInstance(mr_TableQueryCreator)
                .ToString(expression);

            mainQuery.Append(query);

            DbDataReader dataReader = connection.ExecuteReader(mainQuery.ToString());

            IEnumerable<TElement> result = connection
                .GetTableConverter<TElement>()
                .GetObjectsEnumerable(dataReader);

            return result;
        }
    }
}