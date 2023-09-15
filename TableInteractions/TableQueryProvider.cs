using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

using Handy.Converter;
using Handy.Interfaces;
using Handy.TableInteractions;

namespace Handy
{
    internal class TableQueryProvider<T> : IDataQueryProvider<T> where T : class, new()
    {
        private readonly ContextOptions _ContextOptions;
        private readonly TableQueryCreator _TableQueryCreator;
        private readonly TableConverter<T> _TableConverter;

        internal TableQueryProvider(ContextOptions options)
        {
            Type type = typeof(T);
            _ContextOptions = options;
            _TableQueryCreator = TableQueryCreator.GetInstance(type);
            _TableConverter = new TableConverter<T>(_TableQueryCreator.Properties, options.Connection);
        }

        public DbConnection Connection => _ContextOptions.Connection;

        public IExpressionTranslatorProvider ExpressionTranslatorBuilder => _ContextOptions.ExpressionTranslatorBuilder;

        public TableQueryCreator Creator => _TableQueryCreator;

        public TableConverter<T> Converter => _TableConverter;

        public string QueryFromExpression(Expression expression)
        {
            IExpressionTranslatorProvider translatorBuilder = _ContextOptions.ExpressionTranslatorBuilder;

            string query = translatorBuilder
                .CreateInstance(_TableQueryCreator)
                .ToString(expression);

            return query;
        }

        public IDataQueryable<T> CreateQuery(Expression expression) => new Table<T>(this, expression);

        public IEnumerable<T> Execute(Expression expression)
        {
            DbConnection connection = _ContextOptions.Connection;
            string query = QueryFromExpression(expression);
            DbDataReader dataReader = connection.ExecuteReader(query);

            return _TableConverter.Query(dataReader);
        }
    }
}