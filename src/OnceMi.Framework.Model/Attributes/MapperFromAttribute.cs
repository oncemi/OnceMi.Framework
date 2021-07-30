using System;

namespace OnceMi.Framework.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MapperFromAttribute : Attribute
    {
        public Type MapperType { get; private set; }

        public MapperFromAttribute(Type classType)
        {
            this.MapperType = classType;
        }
    }
}
