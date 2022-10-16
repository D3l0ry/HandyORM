using System;
using System.Reflection;

namespace Handy.Converters
{
    /// <summary>
    /// Информация о преобразуемом типе
    /// </summary>
    internal class ConverterTypeInformation
    {
        public ConverterTypeInformation(Type type, bool isTable, bool isArray)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type = type;
            IsTable = isTable;
            IsArray = isArray;
        }

        public Type Type { get; private set; }

        public bool IsTable { get; private set; }

        public bool IsArray { get; private set; }
    }
}