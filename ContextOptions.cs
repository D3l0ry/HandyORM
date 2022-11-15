using System.Data.Common;
using System.Linq.Expressions;

using Handy.Interfaces;

namespace Handy
{
    public class ContextOptions
    {
        internal string ConnectionString { get; set; }

        internal DbConnection Connection { get; set; }

        internal IExpressionTranslatorBuilder ExpressionTranslatorBuilder { get; set; }
    }
}