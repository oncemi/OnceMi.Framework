using Castle.DynamicProxy;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Aop
{
    /// <summary>
    /// 属性拦截器接口
    /// </summary>
    public interface IAttributeAsyncInterceptor : IAsyncInterceptor
    {
        (bool result, List<IAopAttribute> attrs) CanIntercept(IInvocation invocation);
    }
}
