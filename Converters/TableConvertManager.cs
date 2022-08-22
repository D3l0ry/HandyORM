using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;

namespace DatabaseManager
{
    public class TableConvertManager : ConvertManager
    {
        private readonly TableQueryProvider mr_QueryProvider;

        internal TableConvertManager(Type tableType, TableQueryProvider queryProvider) : base(tableType) => mr_QueryProvider = queryProvider;

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

            if (currentColumnAttribute.IsArray)
            {
                propertyGenericType = currentProperty.PropertyType.GetGenericArguments().First();

                if (!propertyGenericType.IsArray)
                {
                    throw new ArrayTypeMismatchException($"Свойство {propertyGenericType} не является массивом, хотя Column объявлен как массив");
                }
            }
            else
            {
                propertyGenericType = currentColumnAttribute.ForeignTable;
            }

            foreignTableType = typeof(ForeignTable<>).MakeGenericType(propertyGenericType);

            if (currentProperty.PropertyType != foreignTableType)
            {
                throw new InvalidCastException($"{foreignTableType} не соответствует типу {currentProperty.PropertyType}");
            }

            TableQueryProvider foreignTableConvertManager = GetOrCreateForeignTableConvertManager(currentProperty, currentColumnAttribute);

            PropertyInfo mainTableForeignKeyProperty = mr_QueryProvider.Creator.PropertyQueryCreator
                .GetProperty(currentColumnAttribute.ForeignKeyName).Key;

            object foreignTable = foreignTableType
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object), typeof(PropertyInfo), typeof(TableQueryProvider), typeof(ColumnAttribute) }, null)
                .Invoke(new object[] { mainTable, mainTableForeignKeyProperty, foreignTableConvertManager, currentColumnAttribute });

            currentProperty.SetValue(mainTable, foreignTable);
        }

        /// <summary>
        /// Создание запроса для внешней таблицы, используемой в главной таблице
        /// </summary>
        /// <param name="currentProperty"></param>
        /// <param name="propertyAttribute"></param>
        /// <returns></returns>
        private TableQueryProvider GetOrCreateForeignTableConvertManager(PropertyInfo currentProperty, ColumnAttribute propertyAttribute)
        {
            if (mr_QueryProvider.Creator.ForeignTables.ContainsKey(currentProperty.Name))
            {
                return mr_QueryProvider.Creator.ForeignTables[currentProperty.Name];
            }

            TableQueryProvider foreignTableConvertManager = new TableQueryProvider(propertyAttribute.ForeignTable, mr_QueryProvider.Connection);

            mr_QueryProvider.Creator.ForeignTables.Add(currentProperty.Name, foreignTableConvertManager);

            return foreignTableConvertManager;
        }

        /// <summary>
        /// Получение объекта из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object GetObject(SqlDataReader dataReader)
        {
            object table = Activator.CreateInstance(mr_Type);

            ushort columnOrdinal = 0;

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in mr_QueryProvider.Creator.PropertyQueryCreator.Properties)
            {
                PropertyInfo currentProperty = currentKeyValuePair.Key;
                ColumnAttribute currentColumnAttribute = currentKeyValuePair.Value;

                if (currentColumnAttribute.IsTable || currentColumnAttribute.IsArray)
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