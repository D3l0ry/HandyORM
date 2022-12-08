using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;

using Handy.Extensions;
using Handy.TableInteractions;

namespace Handy.Converter
{
    /// <summary>
    /// Класс для конвертации объектов из SqlDataReader в тип определяющий таблицу базы данных
    /// </summary>
    internal class TableConverter : DataConverter
    {
        private readonly DbConnection _CurrentContextConnection;
        private readonly TableProperties _TableProperties;

        internal TableConverter(Type tableType, DbConnection connection) : base(tableType)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            TableQueryCreator tableQueryCreator = TableQueryCreator.GetInstance(tableType);

            _CurrentContextConnection = connection;
            _TableProperties = tableQueryCreator.PropertyQueryCreator;
        }

        /// <summary>
        /// Получение объектов из внешней таблицы
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="property"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCreatedInstanceForeignTable(object mainTable, in KeyValuePair<PropertyInfo, ColumnAttribute> property)
        {
            Type foreignTableType = typeof(ForeignTable<>);
            PropertyInfo selectedProperty = property.Key;
            Type selectedPropertyType = selectedProperty.PropertyType;
            ColumnAttribute selectedPropertyColumn = property.Value;

            if (string.IsNullOrWhiteSpace(selectedPropertyColumn.ForeignKeyName))
            {
                throw new NullReferenceException($"Не указан внешний ключ для {selectedProperty.Name}");
            }

            if (!selectedPropertyType.IsGenericType || selectedPropertyType.GetGenericTypeDefinition() != foreignTableType)
            {
                throw new ArgumentException($"Свойство не является типом {foreignTableType.Name}");
            }

            PropertyInfo mainTableForeignKeyProperty = _TableProperties
                .GetProperty(selectedPropertyColumn.ForeignKeyName).Key;

            ConstructorInfo foreignTableConstructor = selectedPropertyType
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]
                    { typeof(object), typeof(PropertyInfo), typeof(DbConnection) }, null);

            object newForeignTableInstance = foreignTableConstructor
                .Invoke(new object[] { mainTable, mainTableForeignKeyProperty, _CurrentContextConnection });

            selectedProperty.SetValue(mainTable, newForeignTableInstance);
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

            foreach (KeyValuePair<PropertyInfo, ColumnAttribute> currentProperty in _TableProperties)
            {
                if (currentProperty.Value.IsTable)
                {
                    SetCreatedInstanceForeignTable(table, currentProperty);

                    continue;
                }

                currentProperty.SetDataReaderValue(table, dataReader);
            }

            return table;
        }
    }
}