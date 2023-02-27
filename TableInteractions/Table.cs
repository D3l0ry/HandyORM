using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Handy.Converter;
using Handy.Interfaces;
using Handy.TableInteractions;

namespace Handy
{
    public class Table<T> : IDataQueryable<T> where T : class, new()
    {
        private readonly TableQueryProvider<T> _TableQueryProvider;
        private readonly Expression _Expression;

        internal Table(ContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _TableQueryProvider = new TableQueryProvider<T>(options);
            _Expression = Expression.Constant(this);
        }

        internal Table(TableQueryProvider<T> tableQueryProvider, Expression expression)
        {
            if (tableQueryProvider == null)
            {
                throw new ArgumentNullException(nameof(tableQueryProvider));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            _TableQueryProvider = tableQueryProvider;
            _Expression = expression;
        }

        public Type ElementType => typeof(T);

        public Expression Expression => _Expression;

        IDataQueryProvider<T> IDataQueryable<T>.Provider => _TableQueryProvider;

        public void Add(T newElement)
        {
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.Properties;

            StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(' ');
            stringBuilder.Append(propertyQueryCreator.GetTableProperties());
            stringBuilder.Append(" VALUES ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesValue(newElement));
            stringBuilder.Append(';');

            _TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public IdType AddAndOutputId<IdType>(T newElement) where IdType : struct
        {
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.Properties;

            StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(' ');
            stringBuilder.Append(propertyQueryCreator.GetTableProperties());
            stringBuilder.Append(" OUTPUT INSERTED.[");
            stringBuilder.Append(propertyQueryCreator.PrimaryKey.Value.Name);
            stringBuilder.Append(']');
            stringBuilder.Append(" VALUES ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesValue(newElement));
            stringBuilder.Append(';');

            DbDataReader dataReader = _TableQueryProvider.Connection.ExecuteReader(stringBuilder.ToString());

            DataConverter<IdType> convertManager = new DataConverter<IdType>();

            return convertManager.GetObject(dataReader);
        }

        public void AddRange(IEnumerable<T> newElements)
        {
            if (newElements == null || newElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(newElements));
            }

            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.Properties;

            DbTransaction transaction = _TableQueryProvider.Connection.BeginTransaction();

            DbCommand sqlCommand = _TableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

                stringBuilder.Append(propertyQueryCreator.GetTableName());
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
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.Properties;

            StringBuilder stringBuilder = new StringBuilder("UPDATE ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(" SET ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesNameAndValue(element));
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(propertyQueryCreator.GetPropertyName(_TableQueryProvider.Creator.Properties.PrimaryKey));
            stringBuilder.Append(" = ");
            stringBuilder.Append(TableProperties.ConvertFieldQuery(_TableQueryProvider.Creator.Properties.PrimaryKey.Key.GetValue(element)));
            stringBuilder.Append(';');

            _TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void Delete(T element)
        {
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.Properties;

            StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(propertyQueryCreator.GetPropertyName(_TableQueryProvider.Creator.Properties.PrimaryKey));
            stringBuilder.Append(" = ");
            stringBuilder.Append(TableProperties.ConvertFieldQuery(_TableQueryProvider.Creator.Properties.PrimaryKey.Key.GetValue(element)));
            stringBuilder.Append(';');

            _TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void Delete(Expression<Func<T, bool>> expression)
        {
            StringBuilder stringBuilder = new StringBuilder($"DELETE FROM ");

            string tableName = _TableQueryProvider.Creator.Properties.GetTableName();

            string expressionTranslator = _TableQueryProvider.ExpressionTranslatorBuilder
                .CreateInstance(_TableQueryProvider.Creator)
                .ToString(expression);

            stringBuilder.Append(tableName);
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(expressionTranslator);

            _TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void DeleteRange(IEnumerable<T> removedElements)
        {
            if (removedElements == null || removedElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(removedElements));
            }

            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.Properties;
            DbTransaction transaction = _TableQueryProvider.Connection.BeginTransaction();

            DbCommand sqlCommand = _TableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                foreach (T currentElement in removedElements)
                {
                    StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");

                    stringBuilder.Append(propertyQueryCreator.GetTableName());
                    stringBuilder.Append(" WHERE ");
                    stringBuilder.Append(propertyQueryCreator.GetPropertyName(_TableQueryProvider.Creator.Properties.PrimaryKey));
                    stringBuilder.Append(" = ");
                    stringBuilder.Append(TableProperties.ConvertFieldQuery(_TableQueryProvider.Creator.Properties.PrimaryKey.Key.GetValue(currentElement)));
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
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.Properties;
            string createElementQuery = $"DELETE FROM {propertyQueryCreator.GetTableName()};";

            _TableQueryProvider.Connection.ExecuteNonQuery(createElementQuery);
        }

        public IEnumerator<T> GetEnumerator() => _TableQueryProvider.Execute(_Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _TableQueryProvider.Execute(_Expression).GetEnumerator();
    }
}