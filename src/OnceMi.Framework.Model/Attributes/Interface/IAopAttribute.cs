using System;

namespace OnceMi.Framework.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public abstract class IAopAttribute : Attribute
    {
        
    }
}
