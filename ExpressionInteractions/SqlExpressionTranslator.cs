using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Handy.QueryInteractions;

namespace Handy.ExpressionInteractions
{
    internal class SqlExpressionTranslator : ExpressionTranslator
    {
        protected override Expression VisitNew(NewExpression node)
        {
            QueryBuilder.Append(GetFieldFromLambda(node));

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            MethodInfo method = node.Method;

            if (method.DeclaringType != typeof(DatabaseQueryable))
            {
                try
                {
                    QueryBuilder.Append(GetFieldFromLambda(node));
                }
                catch
                {
                    throw new FormatException($"Невозможно создать запрос из заданного выражения!\n Ошибка в выражении: {node}");
                }

                return node;
            }

            switch (method.Name)
            {
                case "Where":
                return this.CallWhereMethod(node, QueryBuilder);

                case "First":
                case "FirstOrDefault":
                return this.CallFirstMethod(node, QueryBuilder);
            }

            throw new NotSupportedException($"Указанный метод {method.Name} не поддерживается");
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            Visit(binary.Left);

            switch (binary.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                QueryBuilder.Append(" AND ");
                break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                QueryBuilder.Append(" OR ");
                break;
                case ExpressionType.Equal:
                if (binary.Right is ConstantExpression constantEqual)
                {
                    if (constantEqual.Value == null)
                    {
                        QueryBuilder.Append(" IS NOT ");

                        break;
                    }
                }

                QueryBuilder.Append(" = ");
                break;
                case ExpressionType.NotEqual:
                if (binary.Right is ConstantExpression constantNotEqual)
                {
                    if (constantNotEqual.Value == null)
                    {
                        QueryBuilder.Append(" IS NOT ");

                        break;
                    }
                }

                QueryBuilder.Append(" <> ");
                break;
                case ExpressionType.LessThan:
                QueryBuilder.Append(" < ");
                break;
                case ExpressionType.LessThanOrEqual:
                QueryBuilder.Append(" <= ");
                break;
                case ExpressionType.GreaterThan:
                QueryBuilder.Append(" > ");
                break;
                case ExpressionType.GreaterThanOrEqual:
                QueryBuilder.Append(" >= ");
                break;
                case ExpressionType.IsTrue:
                QueryBuilder.Append(" = TRUE");
                break;
                case ExpressionType.IsFalse:
                QueryBuilder.Append(" = FALSE");
                break;
                default:
                throw new NotSupportedException($"The binary operator '{binary.NodeType}' is not supported");
            }

            Visit(binary.Right);

            return binary;
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            if (member.Expression == null)
            {
                QueryBuilder.Append(GetFieldFromLambda(member));

                return member;
            }

            switch (member.Expression.NodeType)
            {
                case ExpressionType.Parameter:
                GetProperty(member);

                return member;

                case ExpressionType.New:
                Visit(member.Expression);
                return member;

                case ExpressionType.MemberAccess:
                case ExpressionType.Constant:
                case ExpressionType.TypeAs:
                QueryBuilder.Append(GetFieldFromLambda(member));
                return member;
            }

            return member;
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            if (constant.Value is ITableQueryable)
            {
                return constant;
            }

            string field = TablePropertyInformation.ConvertFieldQuery(constant.Value);

            QueryBuilder.Append(field);

            return constant;
        }

        internal void GetProperty(MemberExpression member)
        {
            KeyValuePair<PropertyInfo, ColumnAttribute> selectedProperty = QueryCreator
                .PropertyQueryCreator
                .GetProperty(member.Member as PropertyInfo);

            TablePropertyInformation propertyQueryManager = QueryCreator.PropertyQueryCreator;

            if (selectedProperty.Value.IsForeignColumn && selectedProperty.Value.ForeignTable != null)
            {
                TableQueryCreator tableQueryCreator = TableQueryCreator
                    .GetInstance(selectedProperty.Value.ForeignTable);

                propertyQueryManager = tableQueryCreator.PropertyQueryCreator;
            }

            string tablePropertName = propertyQueryManager.GetPropertyName(selectedProperty);

            QueryBuilder.Append(tablePropertName);
        }

        internal string GetFieldFromLambda(Expression node)
        {
            object field = Expression.Lambda(node).Compile().DynamicInvoke();

            string fieldQuery = TablePropertyInformation.ConvertFieldQuery(field);

            return fieldQuery;
        }

        public override string ToString(Expression expression)
        {
            Visit(expression);

            return QueryBuilder.Append(';').ToString();
        }
    }
}