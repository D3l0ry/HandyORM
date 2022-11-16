using System;

using Handy.Interfaces;
using Handy.QueryInteractions;

namespace Handy.ExpressionInteractions
{
    internal class ExpressionTranslatorProvider<Translator> : IExpressionTranslatorBuilder
        where Translator : ExpressionTranslator, new()
    {
        public ExpressionTranslator CreateInstance(TableQueryCreator tableQueryCreator)
        {
            if (tableQueryCreator == null)
            {
                throw new ArgumentNullException(nameof(tableQueryCreator));
            }

            ExpressionTranslator newExpressionTranslator = Activator.CreateInstance<Translator>();
            newExpressionTranslator.QueryCreator = tableQueryCreator;

            return newExpressionTranslator;
        }
    }
}