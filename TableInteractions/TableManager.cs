using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DatabaseManager.QueryInteractions;
using Microsoft.Data.SqlClient;

namespace DatabaseManager
{
    public class TableManager<Table> : IDatabaseQueryable<Table>
    {
        private readonly TableQueryProvider mr_TableQueryProvider;
        private readonly Expression mr_Expression;

        internal TableManager(SqlConnection sqlConnection) : this(null, null, sqlConnection) { }

        internal TableManager(TableQueryProvider tableQueryProvider, Expression expression, SqlConnection sqlConnection)
        {
            Type tableType = typeof(Table);

            mr_TableQueryProvider = tableQueryProvider ?? new TableQueryProvider(tableType, sqlConnection);
            mr_Expression = expression ?? Expression.Constant(this);
        }

        public Type ElementType => typeof(Table);

        Expression IDatabaseQueryable.Expression => mr_Expression;

        public IDatabaseQueryProvider Provider => mr_TableQueryProvider;

        public void Add(Table newElement)
        {
            TablePropertyQueryManager propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            string createElementQuery = $"INSERT INTO {propertyQueryCreator.GetTableName()} " +
                $"{propertyQueryCreator.GetTableProperties()} VALUES {propertyQueryCreator.GetTablePropertiesValue(newElement)};";

            mr_TableQueryProvider.Connection.ExecuteNonQuery(createElementQuery);
        }

        public IdType AddAndOutputId<IdType>(Table newElement) where IdType : struct
        {
            TablePropertyQueryManager propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            string createElementQuery = $"INSERT INTO {propertyQueryCreator.GetTableName()} " +
                $"{propertyQueryCreator.GetTableProperties()} OUTPUT INSERTED.[{propertyQueryCreator.PrimaryKey.Value.Name}] " +
                $"VALUES {propertyQueryCreator.GetTablePropertiesValue(newElement)};";

            return mr_TableQueryProvider.Execute<IdType>(createElementQuery);
        }

        public void AddRange(IEnumerable<Table> newElements)
        {
            if (newElements is null || newElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(newElements));
            }

            TablePropertyQueryManager propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            SqlTransaction transaction = mr_TableQueryProvider.Connection.BeginTransaction();

            SqlCommand sqlCommand = mr_TableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                foreach (Table currentElement in newElements)
                {
                    string createElementQuery = $"INSERT INTO {propertyQueryCreator.GetTableName()} " +
                        $"{propertyQueryCreator.GetTableProperties()} VALUES {propertyQueryCreator.GetTablePropertiesValue(currentElement)};";

                    sqlCommand.CommandText = createElementQuery;

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

        public void Update(Table element)
        {
            TablePropertyQueryManager propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            string createElementQuery = $"UPDATE {propertyQueryCreator.GetTableName()} SET " +
                $"{propertyQueryCreator.GetTablePropertiesNameAndValue(element)}" +
                $" WHERE {propertyQueryCreator.GetPropertyName(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey)} = " +
                $"{TablePropertyQueryManager.ConvertFieldQuery(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(element))};";

            mr_TableQueryProvider.Connection.ExecuteNonQuery(createElementQuery);
        }

        public void Delete(Table element)
        {
            TablePropertyQueryManager propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            string createElementQuery = $"DELETE FROM {propertyQueryCreator.GetTableName()}" +
                $" WHERE {propertyQueryCreator.GetPropertyName(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey)} = " +
                $"{TablePropertyQueryManager.ConvertFieldQuery(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(element))};";

            mr_TableQueryProvider.Connection.ExecuteNonQuery(createElementQuery);
        }

        public void Delete(Expression<Func<Table, bool>> expression)
        {
            StringBuilder createElementQuery = new StringBuilder($"DELETE {mr_TableQueryProvider.Translator.Translate(expression, false)}");

            mr_TableQueryProvider.Connection.ExecuteNonQuery(createElementQuery.ToString());
        }

        public void DeleteRange(IEnumerable<Table> removedElements)
        {
            if (removedElements is null || removedElements.Count() == 0)
            {
                throw new ArgumentNullException(nameof(removedElements));
            }

            TablePropertyQueryManager propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            SqlTransaction transaction = mr_TableQueryProvider.Connection.BeginTransaction();

            SqlCommand sqlCommand = mr_TableQueryProvider.Connection.CreateCommand();
            sqlCommand.Transaction = transaction;

            try
            {
                foreach (Table currentElement in removedElements)
                {
                    string createElementQuery = $"DELETE FROM {propertyQueryCreator.GetTableName()}" +
                        $" WHERE {propertyQueryCreator.GetPropertyName(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey)} = " +
                        $"{TablePropertyQueryManager.ConvertFieldQuery(mr_TableQueryProvider.Creator.PropertyQueryCreator.PrimaryKey.Key.GetValue(currentElement))};";

                    sqlCommand.CommandText = createElementQuery;

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
            TablePropertyQueryManager propertyQueryCreator = mr_TableQueryProvider.Creator.PropertyQueryCreator;

            string createElementQuery = $"DELETE FROM {propertyQueryCreator.GetTableName()};";

            mr_TableQueryProvider.Connection.ExecuteNonQuery(createElementQuery);
        }

        public IEnumerator<Table> GetEnumerator() => ((IEnumerable<Table>)mr_TableQueryProvider.Execute<Table[]>(mr_Expression)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}