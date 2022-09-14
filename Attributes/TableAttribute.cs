using System;

namespace DatabaseManager
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string name) => Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));

        /// <summary>
        /// Имя таблицы в базе данных
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Имя схемы таблицы в базе данных
        /// </summary>
        public string Schema { get; set; }
    }
}