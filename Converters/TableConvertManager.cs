using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;

using Handy.QueryInteractions;

namespace Handy.Converters
{
    /// <summary>
    /// Класс для конвертации объектов из SqlDataReader в тип определяющий таблицу базы данных
    /// </summary>
    public class TableConvertManager : ConvertManager
    {
        private readonly DbConnection mr_Connection;
        private readonly TablePropertyQueryManager mr_PropertyQueryManager;

        internal TableConvertManager(Type tableType, DbConnection connection) : base(tableType)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            TableQueryCreator tableQueryCreator = TableQueryCreator.GetOrCreateTableQueryCreator(tableType);

            mr_Connection = connection;
            mr_PropertyQueryManager = tableQueryCreator.PropertyQueryCreator;
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
            Type foreignTableType = typeof(ForeignTable<>);

            if (currentProperty.PropertyType.GetGenericTypeDefinition() != foreignTableType)
            {
                throw new ArgumentException($"Свойство не является типом {foreignTableType.Name}");
            }

            PropertyInfo mainTableForeignKeyProperty = mr_PropertyQueryManager
                .GetProperty(currentColumnAttribute.ForeignKeyName).Key;

            object foreignTable = currentProperty.PropertyType
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
                    { typeof(object), typeof(PropertyInfo), typeof(DbConnection) }, null)
                .Invoke(new object[] { mainTable, mainTableForeignKeyProperty, mr_Connection });

            currentProperty.SetValue(mainTable, foreignTable);
        }

        /// <summary>
        /// Получение объекта из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override object GetInternalObject(DbDataReader dataReader)
        {
            object table = Activator.CreateInstance(ObjectType);

            KeyValuePair<PropertyInfo, ColumnAttribute>[] tableProperties = mr_PropertyQueryManager.Properties;

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentKeyValuePair in tableProperties)
            {
                PropertyInfo currentProperty = currentKeyValuePair.Key;
                ColumnAttribute currentColumnAttribute = currentKeyValuePair.Value;

                if (currentColumnAttribute.IsTable)
                {
                    if (string.IsNullOrWhiteSpace(currentColumnAttribute.ForeignKeyName))
                    {
                        throw new NullReferenceException($"Не указан внешний ключ для {currentProperty.Name}");
                    }

                    CreateForeignTable(table, currentProperty, currentColumnAttribute);

                    continue;
                }

                int columnOrdinal = dataReader.GetOrdinal(currentColumnAttribute.Name);

                object readerValue = dataReader.GetValue(columnOrdinal);

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