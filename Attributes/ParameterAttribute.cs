using System;

namespace Handy
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterAttribute : Attribute
    {
        /// <summary>
        /// Имя аргумента в хранимой процедуре
        /// </summary>
        internal string Name { get; set; }

        public ParameterAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }
    }
}