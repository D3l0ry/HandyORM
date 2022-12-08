using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Handy.Interfaces;
using Handy.TableInteractions;

namespace Handy
{
    internal class TableQueryProvider : IQueryProvider
    {
        private readonly ContextOptions _ContextOptions;
        private readonly TableQueryCreator _TableQueryCreator;

        internal TableQueryProvider(Type tableType, ContextOptions options)
        {
            _ContextOptions = options;
            _TableQueryCreator = TableQueryCreator.GetInstance(tableType);
        }

        public DbConnection Connection => _ContextOptions.Connection;

        public IExpressionTranslatorProvider ExpressionTranslatorBuilder => _ContextOptions.ExpressionTranslatorBuilder;

        public TableQueryCreator Creator => _TableQueryCreator;

        internal string QueryFromExpression(Expression expression)
        {
            StringBuilder mainQuery = new StringBuilder(_TableQueryCreator.MainQuery);
            IExpressionTranslatorProvider translatorBuilder = _ContextOptions.ExpressionTranslatorBuilder;

            string query = translatorBuilder
                .CreateInstance(_TableQueryCreator)
                .ToString(expression);

            mainQuery.Append(query);

            return mainQuery.ToString();
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression) => throw new NotImplementedException();

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TableManager<TElement>(this, expression);

        object IQueryProvider.Execute(Expression expression) => throw new NotImplementedException();

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            DbConnection connection = _ContextOptions.Connection;
            string query = QueryFromExpression(expression);
            DbDataReader dataReader = connection.ExecuteReader(query);

            TResult result = connection.ConvertReader<TResult>(dataReader);

            return result;
        }
    }
}