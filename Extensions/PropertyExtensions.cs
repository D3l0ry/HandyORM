using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Handy.Extensions
{
    internal static class PropertyExtensions
    {
        private static void ThrowExceptionIfPropertyIsNotNullable(this PropertyInfo property, object readerValue)
        {
            Type propertyType = property.PropertyType;

            if (!(readerValue is DBNull))
            {
                return;
            }

            if (!propertyType.IsValueType)
            {
                return;
            }

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return;
            }

            throw new InvalidCastException($"Поле {property.Name} вернуло NULL, тогда как тип не принимает такие значения");
        }

        private static void SetValue(this PropertyInfo property, object obj, string columnName, DbDataReader dataReader)
        {
            object readerValue = dataReader[columnName];

            property.ThrowExceptionIfPropertyIsNotNullable(readerValue);
            property.SetValue(obj, readerValue);
        }

        public static void SetDataReaderValue(this PropertyInfo property, object obj, DbDataReader dataReader) =>
            property.SetValue(obj, property.Name, dataReader);

        public static void SetDataReaderValue(this KeyValuePair<PropertyInfo, ColumnAttribute> property, object obj, DbDataReader dataReader) =>
            property.Key.SetValue(obj, property.Value.Name, dataReader);
    }
}