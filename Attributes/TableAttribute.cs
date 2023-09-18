using System;
using System.Text;

namespace Handy
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        /// <summary>
        /// Имя таблицы в базе данных
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Имя схемы таблицы в базе данных
        /// </summary>
        public string Schema { get; set; }

        public string GetFullTableName()
        {
            StringBuilder tableName = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Schema))
            {
                tableName.Append($"[{Schema}].");
            }

            tableName.Append($"[{Name}]");

            return tableName.ToString();
        }
    }
}