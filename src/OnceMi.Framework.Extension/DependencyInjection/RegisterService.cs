using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using OnceMi.Framework.IService;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using OnceMi.Framework.Model.Attributes;
using System.Linq;
using OnceMi.AspNetCore.AutoInjection;
using OnceMi.Framework.Config;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterService
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            Dictionary<Type, Type> registerDic = new AssemblyLoader(p => p.Name.StartsWith(GlobalConstant.FirstNamespace, StringComparison.OrdinalIgnoreCase))
                .GetInheritInterfaceTypes(typeof(IServiceDependency));
            if (registerDic == null)
            {
                return services;
            }
            foreach (var item in registerDic)
            {
                Type implementeType = item.Value;
                bool isProxy = false;
                MethodInfo[] methods = implementeType.GetMethods();
                foreach (var methodItem in methods)
                {
                    var attrs = methodItem.GetCustomAttributes()?.Where(p => p.GetType().BaseType == typeof(IAopAttribute));
                    if (attrs != null && attrs.Any())
                    {
                        isProxy = true;
                        break;
                    }
                }
                if (isProxy)
                    services.TryAddScopedWithProxied(item.Key, item.Value);
                else
                    services.TryAddScoped(item.Key, item.Value);
            }
            return services;
        }
    }
}
