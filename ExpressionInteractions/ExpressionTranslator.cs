using System.Linq.Expressions;
using System.Text;

using Handy.TableInteractions;

namespace Handy.ExpressionInteractions
{
    public abstract class ExpressionTranslator : ExpressionVisitor
    {
        private readonly StringBuilder _TranslatedQuery = new StringBuilder();

        protected StringBuilder QueryBuilder => _TranslatedQuery;

        protected internal TableQueryCreator QueryCreator { get; internal set; }

        public abstract string ToString(Expression expression);
    }
}