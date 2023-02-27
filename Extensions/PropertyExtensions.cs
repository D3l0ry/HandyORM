using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Handy.Extensions
{
    internal static class PropertyExtensions
    {
        /// <summary>
        /// Выдает исключение, если приссваимое значение равно Null, но свойство не принимает значения типа Null
        /// </summary>
        /// <param name="property"></param>
        /// <param name="readerValue"></param>
        /// <exception cref="InvalidCastException"></exception>
        private static object GetValueOrThrowExceptionIfPropertyIsNotNullableType(this PropertyInfo property, object readerValue)
        {
            Type propertyType = property.PropertyType;

            if (!(readerValue is DBNull))
            {
                return readerValue;
            }

            if (!propertyType.IsValueType)
            {
                return null;
            }

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return null;
            }

            throw new InvalidCastException($"Поле {property.Name} вернуло NULL, тогда как тип не принимает такие значения");
        }

        private static void SetValue(this PropertyInfo property, object obj, string columnName, DbDataReader dataReader)
        {
            object readerValue = dataReader[columnName];
            object value = property.GetValueOrThrowExceptionIfPropertyIsNotNullableType(readerValue);

            property.SetValue(obj, value);
        }

        public static void SetDataReaderValue(this PropertyInfo property, object obj, DbDataReader dataReader) =>
            property.SetValue(obj, property.Name, dataReader);

        public static void SetDataReaderValue(this KeyValuePair<PropertyInfo, ColumnAttribute> property, object obj, DbDataReader dataReader) =>
            property.Key.SetValue(obj, property.Value.Name, dataReader);
    }
}