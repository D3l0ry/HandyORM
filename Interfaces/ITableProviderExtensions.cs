using Handy.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace Handy.Interfaces
{
    internal interface ITableProviderExtensions
    {
        SqlConnection Connection { get; }

        TableQueryCreator Creator { get; }

        ExpressionTranslator Translator { get; }
    }
}