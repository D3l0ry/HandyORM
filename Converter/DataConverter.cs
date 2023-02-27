using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Handy.Extensions;
using Handy.Interfaces;

namespace Handy
{
    /// <summary>
    /// Класс для конвертации объектов из SqlDataReader
    /// </summary>
    public class DataConverter<T> : IDataConverter<T> where T : new()
    {
        private readonly Type _ObjectType;
        private readonly PropertyInfo[] _TypeProperties;

        /// <summary>
        /// Инициализатор
        /// </summary>
        /// <param name="type">Тип объекта, к которому нужно привести объекты из SqlDataReader</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DataConverter()
        {
            _ObjectType = typeof(T);
            _TypeProperties = _ObjectType.GetProperties();
        }

        /// <summary>
        /// Тип объекта
        /// </summary>
        protected Type ObjectType => _ObjectType;

        /// <summary>
        /// Свойства объекта
        /// </summary>
        protected PropertyInfo[] Properties => _TypeProperties;

        protected virtual Func<DbDataReader, T> GetDefinedFunctionGetterObject(DbDataReader dataReader)
        {
            if(dataReader.FieldCount == 1)
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
        protected virtual T GetInternalObject(DbDataReader dataReader)
        {
            T table = new T();

            foreach(PropertyInfo currentProperty in _TypeProperties)
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
        protected virtual T GetInternalSimpleObject(DbDataReader dataReader)
        {
            object value = dataReader.GetValue(0);

            if(value is DBNull)
            {
                return default;
            }

            return (T)dataReader.GetValue(0);
        }

        public virtual IEnumerable<T> Query(DbDataReader dataReader)
        {
            Func<DbDataReader, T> function;

            using(dataReader)
            {
                if(!dataReader.HasRows)
                {
                    yield break;
                }

                function = GetDefinedFunctionGetterObject(dataReader);

                while(dataReader.Read())
                {
                    T newObject = function(dataReader);

                    yield return newObject;
                }
            }
        }

        /// <summary>
        /// Получение массива объектов из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public virtual T[] GetObjects(DbDataReader dataReader) => Query(dataReader).ToArray();

        /// <summary>
        /// Получение объекта из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual T GetObject(DbDataReader dataReader) => Query(dataReader).FirstOrDefault();
    }
}