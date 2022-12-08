using Handy.ExpressionInteractions;
using Handy.TableInteractions;

namespace Handy.Interfaces
{
    internal interface IExpressionTranslatorProvider
    {
        ExpressionTranslator CreateInstance(TableQueryCreator tableQueryCreator);
    }
}