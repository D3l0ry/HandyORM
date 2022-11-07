using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

using Handy.QueryInteractions;

using Microsoft.Data.SqlClient;

namespace Handy.Converters
{
    /// <summary>
    /// Класс для конвертации объектов из SqlDataReader в тип определяющий таблицу базы данных
    /// </summary>
    public class TableConvertManager : ConvertManager
    {
        private readonly SqlConnection mr_Connection;
        private readonly TableQueryCreator mr_QueryCreator;

        internal TableConvertManager(Type tableType, SqlConnection connection) : base(tableType)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            mr_Connection = connection;
            mr_QueryCreator = TableQueryCreator.GetOrCreateTableQueryCreator(tableType);
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
                throw new ArgumentException("Свойство не является типом ForeignTable");
            }

            PropertyInfo mainTableForeignKeyProperty = mr_QueryCreator.PropertyQueryCreator
                .GetProperty(currentColumnAttribute.ForeignKeyName).Key;

            object foreignTable = currentProperty.PropertyType
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
                    { typeof(object), typeof(PropertyInfo), typeof(SqlConnection) }, null)
                .Invoke(new object[] { mainTable, mainTableForeignKeyProperty, mr_Connection });

            currentProperty.SetValue(mainTable, foreignTable);
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

            KeyValuePair<PropertyInfo, ColumnAttribute>[] tableProperties = mr_QueryCreator.PropertyQueryCreator.Properties;

            int columnOrdinal = 0;

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