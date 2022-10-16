using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Handy.Converters;

namespace Handy.Extensions
{
    internal static class TypeExtensions
    {
        public static ConverterTypeInformation GetConvertTypeInformation(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            bool isTable;
            bool isArray = false;
            Type elementType = type;

            if (type.IsArray)
            {
                isArray = true;
                elementType = type.GetElementType();
            }

            isTable = elementType.GetCustomAttribute<TableAttribute>() != null;

            ConverterTypeInformation typeInformation = new ConverterTypeInformation(elementType, isTable, isArray);

            return typeInformation;
        }
    }
}