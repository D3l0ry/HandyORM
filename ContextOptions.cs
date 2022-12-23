using System.Data.Common;

using Handy.Interfaces;

namespace Handy
{
    internal class ContextOptions
    {
        public string ConnectionString { get; set; }

        public DbConnection Connection { get; set; }

        public IExpressionTranslatorProvider ExpressionTranslatorBuilder { get; set; }
    }
}