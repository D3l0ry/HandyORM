using System;
using System.Text;

namespace Handy.Extensions
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendStringArray(this StringBuilder stringBuilder, string[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            for (int index = 0; index < values.Length; index++)
            {
                string currentString = values[index];

                stringBuilder.Append(currentString);
            }

            return stringBuilder;
        }
    }
}