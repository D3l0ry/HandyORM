using System;
using System.Data.Common;

using Handy.ExpressionInteractions;

namespace Handy
{
    public class ContextOptionsBuilder
    {
        private readonly ContextOptions _contextOptions;

        public ContextOptionsBuilder() => _contextOptions = new ContextOptions();

        public ContextOptionsBuilder UseConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _contextOptions.ConnectionString = connectionString;

            return this;
        }

        public ContextOptionsBuilder UseConnection(DbConnection connection)
        {
            _contextOptions.Connection = connection;
            _contextOptions.ConnectionString = connection?.ConnectionString;

            return this;
        }

        public ContextOptionsBuilder UseConnection<T>() where T : DbConnection, new()
        {
            _contextOptions.Connection = new T();

            return this;
        }

        public ContextOptionsBuilder UseConnection<T>(string connectionString) where T : DbConnection, new()
        {
            _contextOptions.Connection = new T();
            _contextOptions.ConnectionString = connectionString;

            return this;
        }

        public ContextOptionsBuilder UseExpression<Translator>() where Translator : ExpressionTranslator
        {
            _contextOptions.ExpressionTranslatorBuilder = new ExpressionTranslatorProvider<Translator>();

            return this;
        }

        internal ContextOptions Build()
        {
            if (string.IsNullOrWhiteSpace(_contextOptions.ConnectionString))
            {
                throw new ArgumentNullException(nameof(_contextOptions.ConnectionString));
            }

            if (_contextOptions.Connection == null)
            {
                throw new ArgumentNullException(nameof(_contextOptions.Connection));
            }

            if (_contextOptions.ExpressionTranslatorBuilder == null)
            {
                throw new ArgumentNullException(nameof(_contextOptions.ExpressionTranslatorBuilder));
            }

            return _contextOptions;
        }
    }
}