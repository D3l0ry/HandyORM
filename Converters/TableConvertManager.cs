using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

using DatabaseManager.Interfaces;
using DatabaseManager.QueryInteractions;
using DatabaseManager.TableInteractions;

using Microsoft.Data.SqlClient;

namespace DatabaseManager
{
    public class TableConvertManager : ConvertManager
    {
        private readonly ITableProviderExtensions mr_TableProviderExtensions;
        private readonly Dictionary<string, ITableProviderExtensions> mr_ForeignTableProviderExtensions;

        internal TableConvertManager(Type tableType, ITableProviderExtensions tableProvider) : base(tableType)
        {
            mr_TableProviderExtensions = tableProvider;
            mr_ForeignTableProviderExtensions = new Dictionary<string, ITableProviderExtensions>();
        }

        /// <summary>
        /// Получение объектов из внешней таблицы
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="currentProperty"></param>
        /// <param name="currentColumnAttribute"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateForeignTable(object mainTable, PropertyInfo currentProperty, ColumnAttribute currentColumnAttribute)
        {
            Type foreignTableType;
            Type propertyGenericType;

            propertyGenericType = currentColumnAttribute.ForeignTable;

            foreignTableType = typeof(ForeignTable<>).MakeGenericType(propertyGenericType);

            if (currentProperty.PropertyType != foreignTableType)
            {
                throw new InvalidCastException($"{foreignTableType} не соответствует типу {currentProperty.PropertyType}");
            }

            PropertyInfo mainTableForeignKeyProperty = mr_TableProviderExtensions.Creator.PropertyQueryCreator
                .GetProperty(currentColumnAttribute.ForeignKeyName).Key;

            ITableProviderExtensions foreignTableProviderExtensions =
                GetOrCreateForeignTableProviderExtensions(currentColumnAttribute);

            object foreignTable = foreignTableType
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
                    { typeof(object), typeof(PropertyInfo), typeof(ITableProviderExtensions), typeof(ColumnAttribute) }, null)
                .Invoke(new object[] { mainTable, mainTableForeignKeyProperty, foreignTableProviderExtensions, currentColumnAttribute });

            currentProperty.SetValue(mainTable, foreignTable);
        }

        private ITableProviderExtensions GetOrCreateForeignTableProviderExtensions(ColumnAttribute propertyAttribute)
        {
            if (mr_ForeignTableProviderExtensions.ContainsKey(propertyAttribute.ForeignTable.Name))
            {
                return mr_ForeignTableProviderExtensions[propertyAttribute.ForeignTable.Name];
            }

            TableQueryCreator foreignTableQueryCreator = mr_TableProviderExtensions.Creator
                .GetOrCreateForeignTableQueryCreator(propertyAttribute);

            TableProviderExtensions newForeignTableProviderExtensions =
                new TableProviderExtensions(propertyAttribute.ForeignTable, mr_TableProviderExtensions.Connection, foreignTableQueryCreator);

            mr_ForeignTableProviderExtensions.Add(propertyAttribute.ForeignTable.Name, newForeignTableProviderExtensions);

            return newForeignTableProviderExtensions;
        }

        /// <summary>
        /// Получение объекта из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object GetInternalObject(SqlDataReader dataReader)
        {
            object table = Activator.CreateInstance(ObjectType);

            ushort columnOrdinal = 0;

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in mr_TableProviderExtensions.Creator.PropertyQueryCreator.Properties)
            {
                PropertyInfo currentProperty = currentKeyValuePair.Key;
                ColumnAttribute currentColumnAttribute = currentKeyValuePair.Value;

                if (currentColumnAttribute.IsTable)
                {
                    if (string.IsNullOrWhiteSpace(currentColumnAttribute.ForeignKeyName))
                    {
                        throw new NullReferenceException($"Не указан внешний ключ для {currentProperty.Name}");
                    }

                    if (currentColumnAttribute.ForeignTable is null)
                    {
                        throw new NullReferenceException($"Не указан тип внешней таблицы для {currentProperty.Name}");
                    }

                    CreateForeignTable(table, currentProperty, currentColumnAttribute);

                    continue;
                }

                object readerValue = dataReader.GetValue(columnOrdinal++);

                if (readerValue is DBNull)
                {
                    continue;
                }

                currentProperty.SetValue(table, readerValue);
            }

            return table;
        }
    }
}