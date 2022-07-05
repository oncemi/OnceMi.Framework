using System;

namespace OnceMi.Framework.Model.Attributes
{
    /// <summary>
    /// 跳过打包
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NoPackageAttribute : Attribute
    {

    }
}
