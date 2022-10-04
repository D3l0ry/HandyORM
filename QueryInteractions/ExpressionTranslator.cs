using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Handy.InternalInteractions;

namespace Handy.QueryInteractions
{
    internal class ExpressionTranslator : ExpressionVisitor
    {
        private readonly TableQueryCreator mr_TableQueryCreator;
        private StringBuilder m_TranslatedQuery;

        public ExpressionTranslator(TableQueryCreator tableQueryCreator) => mr_TableQueryCreator = tableQueryCreator;

        internal string Translate(Expression expression, bool getMainQuery = true)
        {
            if (getMainQuery)
            {
                m_TranslatedQuery = new StringBuilder(mr_TableQueryCreator.MainQuery);
            }
            else
            {
                string mainQuery = $"FROM {mr_TableQueryCreator.PropertyQueryCreator.GetTableName()} WHERE ";

                m_TranslatedQuery = new StringBuilder(mainQuery);
            }

            Visit(expression);

            return m_TranslatedQuery.Append(';').ToString();
        }

        protected override Expression VisitNew(NewExpression node)
        {
            m_TranslatedQuery.Append(GetFieldFromLambda(node));

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
                        return this.CallContainsMethod(node, m_TranslatedQuery);
                    }
                    else
                    {
                        m_TranslatedQuery.Append(GetFieldFromLambda(node));
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
                return this.CallWhereMethod(node, m_TranslatedQuery);

                case "First":
                case "FirstOrDefault":
                return this.CallFirstMethod(node, m_TranslatedQuery);
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
                m_TranslatedQuery.Append(" AND ");
                break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                m_TranslatedQuery.Append(" OR ");
                break;
                case ExpressionType.Equal:
                m_TranslatedQuery.Append(" = ");
                break;
                case ExpressionType.NotEqual:
                if (binary.Right is ConstantExpression constant)
                {
                    if (constant.Value is null)
                    {
                        m_TranslatedQuery.Append(" IS NOT ");

                        break;
                    }
                }

                m_TranslatedQuery.Append(" <> ");
                break;
                case ExpressionType.LessThan:
                m_TranslatedQuery.Append(" < ");
                break;
                case ExpressionType.LessThanOrEqual:
                m_TranslatedQuery.Append(" <= ");
                break;
                case ExpressionType.GreaterThan:
                m_TranslatedQuery.Append(" > ");
                break;
                case ExpressionType.GreaterThanOrEqual:
                m_TranslatedQuery.Append(" >= ");
                break;
                case ExpressionType.IsTrue:
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
                m_TranslatedQuery.Append(GetFieldFromLambda(member));

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
                m_TranslatedQuery.Append(GetFieldFromLambda(member));
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

            m_TranslatedQuery.Append(TablePropertyQueryManager.ConvertFieldQuery(constant.Value));

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
                tableProperty = InternalStaticArrays.GetOrCreateTableQueryCreator(selectedProperty.Value.ForeignTable)
                   .PropertyQueryCreator
                   .GetPropertyName(selectedProperty);
            }
            else
            {
                tableProperty = mr_TableQueryCreator
                    .PropertyQueryCreator
                    .GetPropertyName(selectedProperty);
            }

            m_TranslatedQuery.Append(tableProperty);
        }

        internal string GetFieldFromLambda(Expression node)
        {
            object field = Expression.Lambda(node).Compile().DynamicInvoke();

            string fieldQuery = TablePropertyQueryManager.ConvertFieldQuery(field);

            return fieldQuery;
        }
    }
}