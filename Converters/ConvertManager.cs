using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Data.SqlClient;

namespace Handy
{
    /// <summary>
    /// Класс для конвертации объектов из SqlDataReader
    /// </summary>
    public class ConvertManager
    {
        private readonly Type mr_Type;

        /// <summary>
        /// Инициализатор
        /// </summary>
        /// <param name="type">Тип объекта, к которому нужно привести объекты из SqlDataReader</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConvertManager(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            mr_Type = type;
        }

        /// <summary>
        /// Тип объекта
        /// </summary>
        protected Type ObjectType => mr_Type;

        /// <summary>
        /// Конвертирует поля из строки SqlDataReader в объекты выбранного типа
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        protected virtual object GetInternalObject(SqlDataReader dataReader)
        {
            if (dataReader.FieldCount == 1)
            {
                return dataReader.GetValue(0);
            }

            object table = Activator.CreateInstance(mr_Type);

            foreach (PropertyInfo currentProperty in mr_Type.GetProperties())
            {
                object readerValue = dataReader[currentProperty.Name];

                if (readerValue is DBNull)
                {
                    continue;
                }

                currentProperty.SetValue(table, readerValue);
            }

            return table;
        }

        /// <summary>
        /// Получение массива объектов из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IEnumerable GetObjectsEnumerable(SqlDataReader dataReader)
        {
            using (dataReader)
            {
                while (dataReader.Read())
                {
                    object newObject = GetInternalObject(dataReader);

                    yield return newObject;
                }
            }
        }

        /// <summary>
        /// Получение массива объектов из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public virtual IEnumerable GetObjects(SqlDataReader dataReader)
        {
            IList list = (IList)Activator
                .CreateInstance(typeof(List<>)
                .MakeGenericType(mr_Type));

            while (dataReader.Read())
            {
                object newObject = GetInternalObject(dataReader);

                list.Add(newObject);
            }

            Type enumerableType = typeof(Enumerable);

            object cast = enumerableType
                .GetMethod("Cast")
                .MakeGenericMethod(mr_Type)
                .Invoke(null, new object[] { list });

            IEnumerable result = (IEnumerable)enumerableType
                .GetMethod("ToArray")
                .MakeGenericMethod(mr_Type)
                .Invoke(null, new object[] { cast });

            dataReader.Close();

            return result;
        }

        /// <summary>
        /// Получение объекта из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object GetObject(SqlDataReader dataReader)
        {
            object value = null;

            if (dataReader.Read())
            {
                value = GetInternalObject(dataReader);
            }
            else
            {
                if (mr_Type.IsValueType)
                {
                    value = Activator.CreateInstance(mr_Type);
                }
            }

            dataReader.Close();

            return value;
        }
    }
}