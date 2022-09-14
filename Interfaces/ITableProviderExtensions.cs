using DatabaseManager.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace DatabaseManager.Interfaces
{
    internal interface ITableProviderExtensions
    {
        SqlConnection Connection { get; }

        TableQueryCreator Creator { get; }

        ExpressionTranslator Translator { get; }

        TableConvertManager Converter { get; }
    }
}