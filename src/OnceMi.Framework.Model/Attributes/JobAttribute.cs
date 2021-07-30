using FreeSql;
using System;
using System.Data;

namespace OnceMi.Framework.Model.Attributes
{
    /// <summary>
    /// 方法执行完成之后清理指定key的缓存
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class JobAttribute : Attribute
    {

    }
}
