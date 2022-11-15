using System.Linq.Expressions;
using System.Text;

using Handy.QueryInteractions;

namespace Handy.ExpressionInteractions
{
    public abstract class ExpressionTranslator : ExpressionVisitor
    {
        private readonly StringBuilder mr_TranslatedQuery;

        public ExpressionTranslator() => mr_TranslatedQuery = new StringBuilder();

        protected StringBuilder QueryBuilder => mr_TranslatedQuery;

        protected internal TableQueryCreator QueryCreator { get; internal set; }

        public abstract string ToString(Expression expression);
    }
}