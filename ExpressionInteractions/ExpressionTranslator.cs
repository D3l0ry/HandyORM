using System;
using System.Linq.Expressions;
using System.Text;

using Handy.TableInteractions;

namespace Handy.ExpressionInteractions
{
    public abstract class ExpressionTranslator : ExpressionVisitor
    {
        private readonly TableQueryCreator _QueryCreator;
        private readonly StringBuilder _TranslatedQuery;

        protected ExpressionTranslator(TableQueryCreator queryCreator) 
        {
            if (queryCreator == null)
            {
                throw new ArgumentNullException(nameof(queryCreator));
            }

            _QueryCreator = queryCreator;
            _TranslatedQuery = new StringBuilder(queryCreator.MainQuery);
        }

        protected StringBuilder QueryBuilder => _TranslatedQuery;

        protected internal TableQueryCreator QueryCreator => _QueryCreator;

        public abstract string ToString(Expression expression);
    }
}