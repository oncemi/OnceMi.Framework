using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Linq;
using OnceMi.Framework.Model.Attributes;
using OnceMi.AspNetCore.AutoInjection;
using OnceMi.Framework.Config;
using OnceMi.Framework.IRepository;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterRepository
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            Dictionary<Type, Type> registerDic = new AssemblyLoader(p => p.Name.StartsWith(GlobalConstant.FirstNamespace, StringComparison.OrdinalIgnoreCase))
                .GetInheritInterfaceTypes(typeof(IRepositoryDependency));
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
