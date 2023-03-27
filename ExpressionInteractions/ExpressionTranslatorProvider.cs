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
            ExpressionTranslator newExpressionTranslator = (ExpressionTranslator)typeof(Translator)
                .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[]
                    {
                        typeof(TableQueryCreator)
                    }, null)
                .Invoke(new[] { tableQueryCreator });

            return newExpressionTranslator;
        }
    }
}