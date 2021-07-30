using System;

namespace OnceMi.Framework.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MapperToAttribute : Attribute
    {
        public Type MapperType { get; private set; }

        public MapperToAttribute(Type classType)
        {
            this.MapperType = classType;
        }
    }
}
