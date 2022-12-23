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
        }

        /// <summary>
        /// Тип объекта
        /// </summary>
        protected Type ObjectType => _ObjectType;

        protected virtual Func<DbDataReader, object> GetDefinedFunctionGetterObject(DbDataReader dataReader)
        {
            if (dataReader.FieldCount == 1)
            {
                return GetInternalSimpleObject;
            }

            return GetInternalObject;
        }

        /// <summary>
        /// Конвертирует поля из строки SqlDataReader в объекты выбранного типа
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        protected virtual object GetInternalObject(DbDataReader dataReader)
        {
            object table = Activator.CreateInstance(_ObjectType);
            PropertyInfo[] properties = _ObjectType.GetProperties();

            foreach (PropertyInfo currentProperty in properties)
            {
                currentProperty.SetDataReaderValue(table, dataReader);
            }

            return table;
        }

        /// <summary>
        /// Конвертирует поле из строки SqlDataReader в объект выбранного типа (Тип должен быть такого же типа, который получается из SqlDataReader)
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        protected virtual object GetInternalSimpleObject(DbDataReader dataReader) => dataReader.GetValue(0);

        /// <summary>
        /// Получение массива объектов из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public virtual IEnumerable GetObjects(DbDataReader dataReader)
        {
            Func<DbDataReader, object> function;
            IList list = (IList)Activator
                .CreateInstance(typeof(List<>)
                .MakeGenericType(_ObjectType));

            using (dataReader)
            {
                if (!dataReader.HasRows)
                {
                    return list;
                }

                function = GetDefinedFunctionGetterObject(dataReader);

                while (dataReader.Read())
                {
                    object newObject = function(dataReader);

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
            Func<DbDataReader, object> function;

            using (dataReader)
            {
                if (!dataReader.HasRows)
                {
                    if (_ObjectType.IsValueType)
                    {
                        return Activator.CreateInstance(_ObjectType);
                    }

                    return null;
                }

                function = GetDefinedFunctionGetterObject(dataReader);

                if (dataReader.Read())
                {
                    return function(dataReader);
                }
            }

            return null;
        }
    }
}