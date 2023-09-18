using System;
using System.Linq.Expressions;
using System.Text;

using Handy.TableInteractions;

namespace Handy.ExpressionInteractions
{
    public abstract class ExpressionTranslator : ExpressionVisitor
    {
        private readonly TableQueryCreator _queryCreator;
        private readonly StringBuilder _translatedQuery;

        protected ExpressionTranslator(TableQueryCreator queryCreator)
        {
            if (queryCreator == null)
            {
                throw new ArgumentNullException(nameof(queryCreator));
            }

            _queryCreator = queryCreator;
            _translatedQuery = new StringBuilder(queryCreator.MainQuery);
        }

        protected StringBuilder QueryBuilder => _translatedQuery;

        protected internal TableQueryCreator QueryCreator => _queryCreator;

        public abstract string ToString(Expression expression);
    }
}