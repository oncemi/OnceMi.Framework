using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using OnceMi.Framework.Entity;
using FreeSql;
using OnceMi.Framework.Util.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Linq;
using OnceMi.Framework.DependencyInjection.Extensions;
using OnceMi.Framework.Model.Attributes;

namespace OnceMi.Framework.DependencyInjection
{
    public static class RegisterRepository
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            Dictionary<Type, Type> registerDic = new AssemblyHelper()
                .GetInheritInterfaceTypes(typeof(IRepository.IFrameRepository), typeof(IBaseRepository<IEntity, long>));
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
