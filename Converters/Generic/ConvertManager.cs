using System.Collections.Generic;
using System.Data.Common;

namespace Handy.Converters.Generic
{
    internal sealed class ConvertManager<T> : ConvertManager where T : new()
    {
        public ConvertManager() : base(typeof(T)) { }

        /// <summary>
        /// Конвертирует поля из строки SqlDataReader в объекты выбранного типа
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        private new T GetInternalObject(DbDataReader dataReader) => (T)base.GetInternalObject(dataReader);

        /// <summary>
        /// Получение массива объектов из таблицы
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public new IEnumerable<T> GetObjectsEnumerable(DbDataReader dataReader)
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