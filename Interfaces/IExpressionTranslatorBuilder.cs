using System;
using System.Collections.Generic;
using System.Text;

using Handy.ExpressionInteractions;
using Handy.QueryInteractions;

namespace Handy.Interfaces
{
    internal interface IExpressionTranslatorBuilder
    {
        ExpressionTranslator CreateInstance(TableQueryCreator tableQueryCreator);
    }
}