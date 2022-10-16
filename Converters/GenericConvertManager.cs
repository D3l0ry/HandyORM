using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Data.SqlClient;

namespace Handy.Converters
{
    internal sealed class GenericConvertManager<T> : ConvertManager where T : new()
    {
        public GenericConvertManager() : base(typeof(T)) { }

        /// <summary>
        /// Конвертирует поля из строки SqlDataReader в объекты выбранного типа
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        private new T GetInternalObject(SqlDataReader dataReader)
        {
            if (dataReader.FieldCount == 1)
            {
                return (T)dataReader.GetValue(0);
            }

            T element = new T();

            foreach (PropertyInfo currentProperty in ObjectType.GetProperties())
            {
                object readerValue = dataReader[currentProperty.Name];

                if (readerValue is DBNull)
                {
                    continue;
                }

                currentProperty.SetValue(element, readerValue);
            }

            return element;
        }

        /// <summary>
        /// Получение массива объектов из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public new IEnumerable<T> GetObjectsEnumerable(SqlDataReader dataReader)
        {
            using (dataReader)
            {
                while (dataReader.Read())
                {
                    T currentObject = GetInternalObject(dataReader);

                    yield return currentObject;
                }
            }
        }
    }
}