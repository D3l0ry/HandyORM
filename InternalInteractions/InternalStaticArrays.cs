using System;
using System.Collections.Generic;
using System.Text;

using Handy.Interfaces;
using Handy.QueryInteractions;

namespace Handy.InternalInteractions
{
    internal class InternalStaticArrays
    {
        private static readonly Dictionary<Type, TableQueryCreator> ms_TableQueryCreators = new Dictionary<Type, TableQueryCreator>();
        private static readonly Dictionary<Type, string> ms_ForeignTableQuery = new Dictionary<Type, string>();

        public static TableQueryCreator GetOrCreateTableQueryCreator(Type tableType)
        {
            if (ms_TableQueryCreators.TryGetValue(tableType, out TableQueryCreator foundTableQueryCreator))
            {
                return foundTableQueryCreator;
            }

            TableQueryCreator newTableQueryCreator = new TableQueryCreator(tableType);

            ms_TableQueryCreators.Add(tableType, newTableQueryCreator);

            return newTableQueryCreator;
        }

        public static string GetOrCreateForeignTableQuery(ITableProviderExtensions tableProviderExtensions)
        {
            if (ms_ForeignTableQuery.TryGetValue(tableProviderExtensions.TableType, out string foundForeingTableQuery))
            {
                return foundForeingTableQuery;
            }

            TableQueryCreator selectedTableQueryCreator = tableProviderExtensions.Creator;
            TablePropertyQueryManager selectedTablePropertyQueryManager = selectedTableQueryCreator.PropertyQueryCreator;

            StringBuilder queryString = new StringBuilder(selectedTableQueryCreator.MainQuery);

            queryString.Insert(6, " TOP 1 ");
            queryString.Append($" WHERE ");
            queryString.Append(selectedTablePropertyQueryManager.GetPropertyName(selectedTablePropertyQueryManager.PrimaryKey));
            queryString.Append("=");

            string newForeingTableQuery = queryString.ToString();

            ms_ForeignTableQuery.Add(tableProviderExtensions.TableType, newForeingTableQuery);

            return newForeingTableQuery;
        }
    }
}