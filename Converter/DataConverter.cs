using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;

using Handy.Extensions;

namespace Handy
{
    /// <summary>
    /// Класс для конвертации объектов из SqlDataReader
    /// </summary>
    internal class DataConverter
    {
        private readonly Type _ObjectType;
        private readonly PropertyInfo[] _ObjectProperties;

        /// <summary>
        /// Инициализатор
        /// </summary>
        /// <param name="type">Тип объекта, к которому нужно привести объекты из SqlDataReader</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DataConverter(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            _ObjectType = type;
            _ObjectProperties = type.GetProperties();
        }

        /// <summary>
        /// Тип объекта
        /// </summary>
        protected Type ObjectType => _ObjectType;

        /// <summary>
        /// Конвертирует поля из строки SqlDataReader в объекты выбранного типа
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        protected virtual object GetInternalObject(DbDataReader dataReader)
        {
            if (dataReader.FieldCount == 1)
            {
                object value = dataReader.GetValue(0);
                Type valueType = value.GetType();

                if (valueType == _ObjectType)
                {
                    return value;
                }
            }

            object table = Activator.CreateInstance(_ObjectType);

            foreach (PropertyInfo currentProperty in _ObjectProperties)
            {
                currentProperty.SetDataReaderValue(table, dataReader);
            }

            return table;
        }

        /// <summary>
        /// Получение массива объектов из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public virtual IEnumerable GetObjects(DbDataReader dataReader)
        {
            IList list = (IList)Activator
                .CreateInstance(typeof(List<>)
                .MakeGenericType(_ObjectType));

            using (dataReader)
            {
                while (dataReader.Read())
                {
                    object newObject = GetInternalObject(dataReader);

                    list.Add(newObject);
                }
            }

            return list;
        }

        /// <summary>
        /// Получение объекта из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual object GetObject(DbDataReader dataReader)
        {
            using (dataReader)
            {
                if (dataReader.Read())
                {
                    return GetInternalObject(dataReader);
                }

                if (_ObjectType.IsValueType)
                {
                    return Activator.CreateInstance(_ObjectType);
                }
            }

            return null;
        }
    }
}