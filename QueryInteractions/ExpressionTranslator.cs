using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Handy.QueryInteractions
{
    internal class ExpressionTranslator : ExpressionVisitor
    {
        private readonly TableQueryCreator mr_TableQueryCreator;
        private readonly StringBuilder mr_TranslatedQuery;

        public ExpressionTranslator(TableQueryCreator tableQueryCreator)
        {
            mr_TableQueryCreator = tableQueryCreator;
            mr_TranslatedQuery = new StringBuilder();
        }

        internal string Translate(Expression expression)
        {
            Visit(expression);

            return mr_TranslatedQuery.Append(';').ToString();
        }

        protected override Expression VisitNew(NewExpression node)
        {
            mr_TranslatedQuery.Append(GetFieldFromLambda(node));

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            MethodInfo method = node.Method;

            if (method.DeclaringType != typeof(DatabaseQueryable))
            {
                try
                {
                    if (method.Name == "Contains")
                    {
                        return this.CallContainsMethod(node, mr_TranslatedQuery);
                    }
                    else
                    {
                        mr_TranslatedQuery.Append(GetFieldFromLambda(node));
                    }
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
                return this.CallWhereMethod(node, mr_TranslatedQuery);

                case "First":
                case "FirstOrDefault":
                return this.CallFirstMethod(node, mr_TranslatedQuery);
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
                mr_TranslatedQuery.Append(" AND ");
                break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                mr_TranslatedQuery.Append(" OR ");
                break;
                case ExpressionType.Equal:
                mr_TranslatedQuery.Append(" = ");
                break;
                case ExpressionType.NotEqual:
                if (binary.Right is ConstantExpression constant)
                {
                    if (constant.Value is null)
                    {
                        mr_TranslatedQuery.Append(" IS NOT ");

                        break;
                    }
                }

                mr_TranslatedQuery.Append(" <> ");
                break;
                case ExpressionType.LessThan:
                mr_TranslatedQuery.Append(" < ");
                break;
                case ExpressionType.LessThanOrEqual:
                mr_TranslatedQuery.Append(" <= ");
                break;
                case ExpressionType.GreaterThan:
                mr_TranslatedQuery.Append(" > ");
                break;
                case ExpressionType.GreaterThanOrEqual:
                mr_TranslatedQuery.Append(" >= ");
                break;
                case ExpressionType.IsTrue:
                mr_TranslatedQuery.Append(" = TRUE");
                break;
                case ExpressionType.IsFalse:
                mr_TranslatedQuery.Append(" = FALSE");
                break;
                default:
                throw new NotSupportedException($"The binary operator '{binary.NodeType}' is not supported");
            }

            Visit(binary.Right);

            return binary;
        }

        protected override Expression VisitMember(MemberExpression member)
        {
            if (member.Expression is null)
            {
                mr_TranslatedQuery.Append(GetFieldFromLambda(member));

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
                mr_TranslatedQuery.Append(GetFieldFromLambda(member));
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

            mr_TranslatedQuery.Append(TablePropertyQueryManager.ConvertFieldQuery(constant.Value));

            return constant;
        }

        internal void GetProperty(MemberExpression member)
        {
            KeyValuePair<PropertyInfo, ColumnAttribute> selectedProperty = mr_TableQueryCreator
                .PropertyQueryCreator
                .GetProperty(member.Member as PropertyInfo);

            string tableProperty;

            if (selectedProperty.Value.IsForeignColumn && selectedProperty.Value.ForeignTable != null)
            {
                tableProperty = TableQueryCreator.GetOrCreateTableQueryCreator(selectedProperty.Value.ForeignTable)
                   .PropertyQueryCreator
                   .GetPropertyName(selectedProperty);
            }
            else
            {
                tableProperty = mr_TableQueryCreator
                    .PropertyQueryCreator
                    .GetPropertyName(selectedProperty);
            }

            mr_TranslatedQuery.Append(tableProperty);
        }

        internal string GetFieldFromLambda(Expression node)
        {
            object field = Expression.Lambda(node).Compile().DynamicInvoke();

            string fieldQuery = TablePropertyQueryManager.ConvertFieldQuery(field);

            return fieldQuery;
        }
    }
}