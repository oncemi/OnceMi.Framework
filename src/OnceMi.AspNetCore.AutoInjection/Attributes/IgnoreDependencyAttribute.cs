using System;

namespace OnceMi.AspNetCore.AutoInjection
{
    /// <summary>
    /// 忽略依赖注入自动注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class IgnoreDependencyAttribute : Attribute
    {

    }
}
