using System;
using System.Collections.Generic;
using System.Text;

using Handy.Interfaces;
using Handy.QueryInteractions;

namespace Handy.ExpressionInteractions
{
    internal class ExpressionTranslatorProvider<Translator> : IExpressionTranslatorBuilder
        where Translator : ExpressionTranslator, new()
    {
        ExpressionTranslator IExpressionTranslatorBuilder.CreateInstance(TableQueryCreator tableQueryCreator)
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