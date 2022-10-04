using System;

using Handy.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace Handy.Interfaces
{
    internal interface ITableProviderExtensions
    {
        Type TableType { get; }

        SqlConnection Connection { get; }

        TableQueryCreator Creator { get; }

        ExpressionTranslator Translator { get; }

        TableConvertManager Converter { get; }
    }
}