using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Handy.Interfaces;
using Handy.TableInteractions;

namespace Handy
{
    public class Table<T> : IDataQueryable<T> where T : class, new()
    {
        private readonly TableProvider<T> _tableQueryProvider;
        private readonly Expression _expression;

        internal Table(ContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _tableQueryProvider = new TableProvider<T>(options);
            _expression = Expression.Constant(this);
        }

        internal Table(TableProvider<T> tableQueryProvider, Expression expression)
        {
            _tableQueryProvider = tableQueryProvider ?? throw new ArgumentNullException(nameof(tableQueryProvider));
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Type ElementType => typeof(T);

        public Expression Expression => _expression;

        IDataQueryProvider<T> IDataQueryable<T>.Provider => _tableQueryProvider;

        public void Add(T newElement)
        {
            TableProperties propertyQueryCreator = _tableQueryProvider.Creator.Properties;

            StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

            stringBuilder.Append(_tableQueryProvider.Creator.Attribute.GetFullTableName());
            stringBuilder.Append(' ');
            stringBuilder.Append(propertyQueryCreator.GetTableProperties());
            stringBuilder.Append(" VALUES ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesValue(newElement));
            stringBuilder.Append(';');

            _tableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public IdType AddAndOutputId<IdType>(T newElement) where IdType : struct
        {
            TableProperties propertyQueryCreator = _tableQueryProvider.Creator.Properties;

            StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

            stringBuilder.Append(_tableQueryProvider.Creator.Attribute.GetFullTableName());
            stringBuilder.Append(' ');
            stringBuilder.Append(propertyQueryCreator.GetTableProperties());
            stringBuilder.Append(" OUTPUT INSERTED.[");
            stringBuilder.Append(propertyQueryCreator.PrimaryKey.Value.Name);
            stringBuilder.Append(']');
            stringBuilder.Append(" VALUES ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesValue(newElement));
            stringBuilder.Append(';');

            DbDataReader dataReader = _tableQueryProvider.Connection.ExecuteReader(stringBuilder.ToString());

            DataConverter<IdType> convertManager = new DataConverter<IdType>();

            return convertManager.GetObject(dataReader);
        }

        public void AddRange(IEnumerable<T> newElements)
        {
            if (newElements == null || newElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(newElements));
            }

            TableProperties propertyQueryCreator = _tableQueryProvider.Creator.Properties;

            DbTransaction transaction = _tableQueryProvider.Connection.BeginTransaction();

            DbCommand sqlCommand = _tableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

                stringBuilder.Append(_tableQueryProvider.Creator.Attribute.GetFullTableName());
                stringBuilder.Append(' ');
                stringBuilder.Append(propertyQueryCreator.GetTableProperties());
                stringBuilder.Append(" VALUES ");

                foreach (T table in newElements)
                {
                    string newTablePropertiesValue = propertyQueryCreator.GetTablePropertiesValue(table);

                    stringBuilder.Append(newTablePropertiesValue);
                    stringBuilder.Append(',');
                }

                stringBuilder[stringBuilder.Length - 1] = ';';

                sqlCommand.CommandText = stringBuilder.ToString();
                sqlCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (Exception Ex)
            {
                transaction.Rollback();

                throw Ex;
            }
        }

        public void Update(T element)
        {
            TableProperties propertyQueryCreator = _tableQueryProvider.Creator.Properties;

            StringBuilder stringBuilder = new StringBuilder("UPDATE ");

            stringBuilder.Append(_tableQueryProvider.Creator.Attribute.GetFullTableName());
            stringBuilder.Append(" SET ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesNameAndValue(element));
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(propertyQueryCreator.GetPropertyName(_tableQueryProvider.Creator.Properties.PrimaryKey));
            stringBuilder.Append(" = ");
            stringBuilder.Append(TableProperties.ConvertFieldQuery(_tableQueryProvider.Creator.Properties.PrimaryKey.Key.GetValue(element)));
            stringBuilder.Append(';');

            _tableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void Delete(T element)
        {
            TableProperties propertyQueryCreator = _tableQueryProvider.Creator.Properties;

            StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");

            stringBuilder.Append(_tableQueryProvider.Creator.Attribute.GetFullTableName());
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(propertyQueryCreator.GetPropertyName(_tableQueryProvider.Creator.Properties.PrimaryKey));
            stringBuilder.Append(" = ");
            stringBuilder.Append(TableProperties.ConvertFieldQuery(_tableQueryProvider.Creator.Properties.PrimaryKey.Key.GetValue(element)));
            stringBuilder.Append(';');

            _tableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void Delete(Expression<Func<T, bool>> expression)
        {
            StringBuilder stringBuilder = new StringBuilder($"DELETE FROM ");

            string expressionTranslator = _tableQueryProvider.ExpressionTranslatorBuilder
                .CreateInstance(_tableQueryProvider.Creator)
                .ToString(expression);

            stringBuilder.Append(_tableQueryProvider.Creator.Attribute.GetFullTableName());
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(expressionTranslator);

            _tableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void DeleteRange(IEnumerable<T> removedElements)
        {
            if (removedElements == null || removedElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(removedElements));
            }

            TableProperties propertyQueryCreator = _tableQueryProvider.Creator.Properties;
            DbTransaction transaction = _tableQueryProvider.Connection.BeginTransaction();

            DbCommand sqlCommand = _tableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                foreach (T currentElement in removedElements)
                {
                    StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");

                    stringBuilder.Append(_tableQueryProvider.Creator.Attribute.GetFullTableName());
                    stringBuilder.Append(" WHERE ");
                    stringBuilder.Append(propertyQueryCreator.GetPropertyName(_tableQueryProvider.Creator.Properties.PrimaryKey));
                    stringBuilder.Append(" = ");
                    stringBuilder.Append(TableProperties.ConvertFieldQuery(_tableQueryProvider.Creator.Properties.PrimaryKey.Key.GetValue(currentElement)));
                    stringBuilder.Append(';');

                    sqlCommand.CommandText = stringBuilder.ToString();

                    sqlCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception Ex)
            {
                transaction.Rollback();

                throw Ex;
            }
        }

        public void DeleteAll()
        {
            string createElementQuery = $"DELETE FROM {_tableQueryProvider.Creator.Attribute.GetFullTableName()};";

            _tableQueryProvider.Connection.ExecuteNonQuery(createElementQuery);
        }

        public IEnumerator<T> GetEnumerator() => _tableQueryProvider.Execute(_expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _tableQueryProvider.Execute(_expression).GetEnumerator();
    }
}