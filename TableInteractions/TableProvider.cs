using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;

using Handy.Converter;
using Handy.Interfaces;
using Handy.TableInteractions;

namespace Handy
{
    internal class TableProvider<T> : IDataQueryProvider<T> where T : class, new()
    {
        private readonly ContextOptions _contextOptions;
        private readonly TableQueryCreator _tableQueryCreator;
        private readonly TableConverter<T> _tableConverter;

        internal TableProvider(ContextOptions options)
        {
            Type type = typeof(T);
            _contextOptions = options;
            _tableQueryCreator = TableQueryCreator.GetInstance(type);
            _tableConverter = new TableConverter<T>(_tableQueryCreator.Properties, options.Connection);
        }

        public DbConnection Connection => _contextOptions.Connection;

        public IExpressionTranslatorProvider ExpressionTranslatorBuilder => _contextOptions.ExpressionTranslatorBuilder;

        public TableQueryCreator Creator => _tableQueryCreator;

        public TableConverter<T> Converter => _tableConverter;

        public string QueryFromExpression(Expression expression)
        {
            IExpressionTranslatorProvider translatorBuilder = _contextOptions.ExpressionTranslatorBuilder;

            string query = translatorBuilder
                .CreateInstance(_tableQueryCreator)
                .ToString(expression);

            return query;
        }

        public IDataQueryable<T> CreateQuery(Expression expression) => new Table<T>(this, expression);

        public IEnumerable<T> Execute(Expression expression)
        {
            DbConnection connection = _contextOptions.Connection;
            string query = QueryFromExpression(expression);
            DbDataReader dataReader = connection.ExecuteReader(query);

            return _tableConverter.Query(dataReader);
        }
    }
}