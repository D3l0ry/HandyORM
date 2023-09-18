using System;

namespace Handy.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ForeignTableAttribute : Attribute
    {
        public string ThisKey { get; set; }

        public string OtherKey { get; set; }
    }
}