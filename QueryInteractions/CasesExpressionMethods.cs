using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Handy.QueryInteractions
{
    internal static class CasesExpressionMethods
    {
        private static void CreateParameterString(this ExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
        {
            queryString.Append("(");

            visitor.Visit(methodCall.Arguments.Count == 1 ? methodCall.Arguments.First() : methodCall.Arguments.Last());

            queryString.Append(")");
        }

        private static void CreateWhereQuery(StringBuilder queryString) => queryString.Append(!queryString.ToString().Contains("WHERE") ? " WHERE " : " AND ");

        private static MethodCallExpression CallWhereLiteMethod(this ExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
        {
            CreateWhereQuery(queryString);

            visitor.CreateParameterString(methodCall, queryString);

            return methodCall;
        }

        public static MethodCallExpression CallWhereMethod(this ExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
        {
            visitor.Visit(methodCall.Arguments[0]);

            return visitor.CallWhereLiteMethod(methodCall, queryString);
        }

        public static MethodCallExpression CallFirstMethod(this ExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
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

        public static MethodCallExpression CallContainsMethod(this ExpressionTranslator visitor, MethodCallExpression methodCall, StringBuilder queryString)
        {
            visitor.GetProperty((MemberExpression)methodCall.Object);

            object field = Expression.Lambda(methodCall.Arguments.First()).Compile().DynamicInvoke();

            string fieldQuery = field.ToString();

            queryString.Append($" LIKE '%{fieldQuery}%' ");

            return methodCall;
        }
    }
}