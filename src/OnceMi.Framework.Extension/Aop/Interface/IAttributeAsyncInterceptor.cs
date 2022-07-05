using Castle.DynamicProxy;
using OnceMi.Framework.Model.Attributes;
using System.Collections.Generic;

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
