using Microsoft.Extensions.DependencyInjection;

namespace OnceMi.AspNetCore.AutoInjection
{
    /// <summary>
    /// 实现此接口的类型将被注册为<see cref="ServiceLifetime.Scoped"/>模式
    /// </summary>
    [IgnoreDependency]
    public interface IScopeDependency
    {

    }
}