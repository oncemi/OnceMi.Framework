using System;

namespace OnceMi.Framework.Model.Attributes
{
    /// <summary>
    /// 标识这个接口是为一个作业请求接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class JobAttribute : Attribute
    {

    }
}
