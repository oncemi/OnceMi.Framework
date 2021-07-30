using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
