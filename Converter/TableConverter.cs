using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Handy.Extensions;
using Handy.Interfaces;
using Handy.TableInteractions;

namespace Handy.Converter
{
    /// <summary>
    /// Класс для конвертации объектов из SqlDataReader в тип определяющий таблицу базы данных
    /// </summary>
    internal sealed class TableConverter<T> : IDataConverter<T> where T : new()
    {
        private readonly DbConnection _CurrentContextConnection;
        private readonly TableProperties _TableProperties;

        internal TableConverter(DbConnection connection)
        {
            if(connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            TableQueryCreator tableQueryCreator = TableQueryCreator.GetInstance(typeof(T));

            _CurrentContextConnection = connection;
            _TableProperties = tableQueryCreator.Properties;
        }

        internal TableConverter(TableProperties tableProperties, DbConnection connection)
        {
            if(tableProperties == null)
            {
                throw new ArgumentNullException(nameof(tableProperties));
            }

            if(connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _CurrentContextConnection = connection;
            _TableProperties = tableProperties;
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

            if(string.IsNullOrWhiteSpace(selectedPropertyColumn.ForeignKeyName))
            {
                throw new NullReferenceException($"Не указан внешний ключ для {selectedProperty.Name}");
            }

            if(!selectedPropertyType.IsGenericType || selectedPropertyType.GetGenericTypeDefinition() != foreignTableType)
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
        private T GetInternalObject(DbDataReader dataReader)
        {
            T newObject = new T();

            foreach(KeyValuePair<PropertyInfo, ColumnAttribute> currentProperty in _TableProperties)
            {
                if(currentProperty.Value.IsTable)
                {
                    SetCreatedInstanceForeignTable(newObject, currentProperty);

                    continue;
                }

                currentProperty.SetDataReaderValue(newObject, dataReader);
            }

            return newObject;
        }

        public IEnumerable<T> Query(DbDataReader dataReader)
        {
            using(dataReader)
            {
                if(!dataReader.HasRows)
                {
                    yield break;
                }

                while(dataReader.Read())
                {
                    T newObject = GetInternalObject(dataReader);

                    yield return newObject;
                }
            }
        }

        public T[] GetObjects(DbDataReader dataReader) => Query(dataReader).ToArray();

        public T GetObject(DbDataReader dataReader) => Query(dataReader).FirstOrDefault();
    }
}