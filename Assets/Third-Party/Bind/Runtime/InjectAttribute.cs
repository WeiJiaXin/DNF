using System;

namespace Lowy.Bind
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
        private string name;
        public string Name => name;

        public InjectAttribute()
        {
        }

        public InjectAttribute(Enum name) : this(name.ToString())
        {
        }

        public InjectAttribute(string name)
        {
            this.name = name;
        }
    }
}