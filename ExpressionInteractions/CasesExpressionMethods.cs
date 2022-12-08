using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Handy.ExpressionInteractions
{
    internal static class CasesExpressionMethods
    {
        private static void CreateParameterString(this SqlExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
        {
            queryString.Append("(");

            visitor.Visit(methodCall.Arguments.Count == 1 ? methodCall.Arguments.First() : methodCall.Arguments.Last());

            queryString.Append(")");
        }

        private static void CreateWhereQuery(StringBuilder queryString) => queryString.Append(!queryString.ToString().Contains("WHERE") ? " WHERE " : " AND ");

        private static MethodCallExpression CallWhereLiteMethod(this SqlExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
        {
            CreateWhereQuery(queryString);

            visitor.CreateParameterString(methodCall, queryString);

            return methodCall;
        }

        public static MethodCallExpression CallWhereMethod(this SqlExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
        {
            visitor.Visit(methodCall.Arguments[0]);

            return visitor.CallWhereLiteMethod(methodCall, queryString);
        }

        public static MethodCallExpression CallFirstMethod(this SqlExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
        {
            if (queryString.ToString().Contains("TOP"))
            {
                return methodCall;
            }

            visitor.Visit(methodCall.Arguments[0]);

            queryString.Insert(7, " TOP 1 ");

            if (methodCall.Arguments.Count > 1)
            {
                visitor.CallWhereLiteMethod(methodCall, queryString);
            }

            return methodCall;
        }
    }
}