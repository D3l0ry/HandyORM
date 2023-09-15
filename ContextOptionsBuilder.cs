using System;
using System.Data.Common;

using Handy.ExpressionInteractions;

namespace Handy
{
    public class ContextOptionsBuilder
    {
        private readonly ContextOptions m_ContextOptions;

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

        public ContextOptionsBuilder UseConnection<T>() where T : DbConnection, new()
        {
            m_ContextOptions.Connection = new T();

            return this;
        }

        public ContextOptionsBuilder UseConnection<T>(string connection) where T : DbConnection, new()
        {
            if (string.IsNullOrWhiteSpace(connection))
            {
                throw new ArgumentNullException(nameof(connection));
            }

            m_ContextOptions.Connection = new T();
            m_ContextOptions.ConnectionString = connection;

            return this;
        }

        public ContextOptionsBuilder UseExpression<Translator>() where Translator : ExpressionTranslator
        {
            m_ContextOptions.ExpressionTranslatorBuilder = new ExpressionTranslatorProvider<Translator>();

            return this;
        }

        internal ContextOptions Build()
        {
            if (string.IsNullOrWhiteSpace(m_ContextOptions.ConnectionString))
            {
                throw new ArgumentNullException(nameof(m_ContextOptions.ConnectionString));
            }

            if (m_ContextOptions.Connection == null)
            {
                throw new ArgumentNullException(nameof(m_ContextOptions.Connection));
            }

            if (m_ContextOptions.ExpressionTranslatorBuilder == null)
            {
                throw new ArgumentNullException(nameof(m_ContextOptions.ExpressionTranslatorBuilder));
            }

            return m_ContextOptions;
        }
    }
}