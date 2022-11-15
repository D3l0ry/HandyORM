using System;
using System.Data.Common;
using System.Linq.Expressions;

using Handy.ExpressionInteractions;
using Handy.Interfaces;

namespace Handy
{
    public class ContextOptionsBuilder
    {
        protected readonly ContextOptions m_ContextOptions;

        public ContextOptionsBuilder() => m_ContextOptions = new ContextOptions();

        public ContextOptionsBuilder UseConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            m_ContextOptions.ConnectionString = connectionString;

            return this;
        }

        public ContextOptionsBuilder UseConnection(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            m_ContextOptions.Connection = connection;

            return this;
        }

        public ContextOptionsBuilder UseExpression<Translator>() where Translator : ExpressionTranslator, new()
        {
            m_ContextOptions.ExpressionTranslatorBuilder = new ExpressionTranslatorProvider<Translator>();

            return this;
        }

        public ContextOptions Build()
        {
            if (string.IsNullOrWhiteSpace(m_ContextOptions.ConnectionString))
            {
                throw new ArgumentNullException("ConnectionString");
            }

            if (m_ContextOptions.Connection == null)
            {
                throw new ArgumentNullException("Connection");
            }

            if(m_ContextOptions.ExpressionTranslatorBuilder == null)
            {
                m_ContextOptions.ExpressionTranslatorBuilder = new ExpressionTranslatorProvider<SqlExpressionTranslator>();
            }

            return m_ContextOptions;
        }
    }
}