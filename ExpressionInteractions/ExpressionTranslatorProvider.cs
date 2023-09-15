using System;
using System.Reflection;

using Handy.Interfaces;
using Handy.TableInteractions;

namespace Handy.ExpressionInteractions
{
    internal class ExpressionTranslatorProvider<Translator> : IExpressionTranslatorProvider
        where Translator : ExpressionTranslator
    {
        public ExpressionTranslator CreateInstance(TableQueryCreator tableQueryCreator)
        {
            Type[] arguments = new[]
            {
                typeof(TableQueryCreator)
            };

            object[] parameters = new[]
            {
                tableQueryCreator
            };

            ExpressionTranslator newExpressionTranslator = (ExpressionTranslator)typeof(Translator)
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, arguments, null)
                .Invoke(parameters);

            return newExpressionTranslator;
        }
    }
}