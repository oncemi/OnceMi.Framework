using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using OnceMi.Framework.Model.Attributes;
using OnceMi.AspNetCore.AutoInjection;
using OnceMi.Framework.Config;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterRepository
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            //为了引入OnceMi.Framework.IRepository的实现，不然可能无法正确的获取仓储实现
            Type configRepository = typeof(ConfigRepository);
            if (configRepository == null)
            {
                throw new Exception("Can not load repository realization");
            }
            Dictionary<Type, Type> registerDic = new AssemblyLoader(p => p.Name.StartsWith(GlobalConfigConstant.FirstNamespace, StringComparison.OrdinalIgnoreCase))
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
