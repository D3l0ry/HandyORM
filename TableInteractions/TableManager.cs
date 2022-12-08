using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Handy.Converter;
using Handy.TableInteractions;

namespace Handy
{
    public class TableManager<Table> : IQueryable<Table>
    {
        private readonly TableQueryProvider _TableQueryProvider;
        private readonly Expression _Expression;

        internal TableManager(ContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _TableQueryProvider = new TableQueryProvider(typeof(Table), options);
            _Expression = Expression.Constant(this);
        }

        internal TableManager(TableQueryProvider tableQueryProvider, Expression expression)
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

        public Type ElementType => typeof(Table);

        public Expression Expression => _Expression;

        IQueryProvider IQueryable.Provider => _TableQueryProvider;

        private IEnumerable<Table> FromExpression()
        {
            string query = _TableQueryProvider.QueryFromExpression(_Expression);

            return FromSql(query);
        }

        public void Add(Table newElement)
        {
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.PropertyQueryCreator;

            StringBuilder stringBuilder = new StringBuilder("INSERT INTO ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(' ');
            stringBuilder.Append(propertyQueryCreator.GetTableProperties());
            stringBuilder.Append(" VALUES ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesValue(newElement));
            stringBuilder.Append(';');

            _TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public IdType AddAndOutputId<IdType>(Table newElement) where IdType : struct
        {
            Type idType = typeof(IdType);

            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.PropertyQueryCreator;

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

            DataConverter convertManager = new DataConverter(idType);

            return (IdType)convertManager.GetObject(dataReader);
        }

        public void AddRange(IEnumerable<Table> newElements)
        {
            if (newElements == null || newElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(newElements));
            }

            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.PropertyQueryCreator;

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

                foreach (Table table in newElements)
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

        public void Update(Table element)
        {
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.PropertyQueryCreator;

            StringBuilder stringBuilder = new StringBuilder("UPDATE ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(" SET ");
            stringBuilder.Append(propertyQueryCreator.GetTablePropertiesNameAndValue(element));
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(propertyQueryCreator.GetPropertyName(_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey));
            stringBuilder.Append(" = ");
            stringBuilder.Append(TableProperties.ConvertFieldQuery(_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(element)));
            stringBuilder.Append(';');

            _TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void Delete(Table element)
        {
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.PropertyQueryCreator;

            StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");

            stringBuilder.Append(propertyQueryCreator.GetTableName());
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(propertyQueryCreator.GetPropertyName(_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey));
            stringBuilder.Append(" = ");
            stringBuilder.Append(TableProperties.ConvertFieldQuery(_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(element)));
            stringBuilder.Append(';');

            _TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void Delete(Expression<Func<Table, bool>> expression)
        {
            StringBuilder stringBuilder = new StringBuilder($"DELETE FROM ");

            string tableName = _TableQueryProvider.Creator.PropertyQueryCreator.GetTableName();

            string expressionTranslator = _TableQueryProvider.ExpressionTranslatorBuilder
                .CreateInstance(_TableQueryProvider.Creator)
                .ToString(expression);

            stringBuilder.Append(tableName);
            stringBuilder.Append(" WHERE ");
            stringBuilder.Append(expressionTranslator);

            _TableQueryProvider.Connection.ExecuteNonQuery(stringBuilder.ToString());
        }

        public void DeleteRange(IEnumerable<Table> removedElements)
        {
            if (removedElements == null || removedElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(removedElements));
            }

            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.PropertyQueryCreator;
            DbTransaction transaction = _TableQueryProvider.Connection.BeginTransaction();

            DbCommand sqlCommand = _TableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                foreach (Table currentElement in removedElements)
                {
                    StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");

                    stringBuilder.Append(propertyQueryCreator.GetTableName());
                    stringBuilder.Append(" WHERE ");
                    stringBuilder.Append(propertyQueryCreator.GetPropertyName(_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey));
                    stringBuilder.Append(" = ");
                    stringBuilder.Append(TableProperties.ConvertFieldQuery(_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(currentElement)));
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
            TableProperties propertyQueryCreator = _TableQueryProvider.Creator.PropertyQueryCreator;

            string createElementQuery = $"DELETE FROM {propertyQueryCreator.GetTableName()};";

            _TableQueryProvider.Connection.ExecuteNonQuery(createElementQuery);
        }

        public IEnumerable<Table> FromSql(string query)
        {
            DbConnection connection = _TableQueryProvider.Connection;
            TableConverter tableConvertManager = connection.GetTableConverter<Table>();
            DbDataReader dataReader = connection.ExecuteReader(query);

            IEnumerable<Table> value = (IEnumerable<Table>)tableConvertManager.GetObjects(dataReader);

            return value;
        }

        public IEnumerator<Table> GetEnumerator() => FromExpression().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => FromExpression().GetEnumerator();
    }
}