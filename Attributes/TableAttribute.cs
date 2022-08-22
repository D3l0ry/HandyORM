using System;

namespace DatabaseManager
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string name) => Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));

        internal string Name { get; set; }

        public string Schema { get; set; }
    }
}