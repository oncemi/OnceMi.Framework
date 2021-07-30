using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using OnceMi.AspNetCore.AutoInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AutoInjectionServiceCollection
    {
        public static IServiceCollection AddAutoInjection(this IServiceCollection services)
        {
            Injection(services);
            return services;
        }

        private static void Injection(IServiceCollection services)
        {
            List<Assembly> assemblies = DependencyContext.Default.RuntimeLibraries
                .Select(o =>
                {
                    try
                    {
                        return Assembly.Load(new AssemblyName(o.Name));
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(p => p != null)
                .ToList();

            if (assemblies == null || assemblies.Count() == 0)
            {
                throw new Exception("Get assemblies failed.");
            }
            Dictionary<Type, AutoInjectionAttribute> poolDic = new Dictionary<Type, AutoInjectionAttribute>();
            foreach (var assemblyItem in assemblies)
            {
                Type[] types = assemblyItem.GetExportedTypes();
                foreach (var item in types)
                {
                    var attr = item.GetCustomAttribute<AutoInjectionAttribute>();
                    if (attr == null)
                        continue;
                    if (item.IsAbstract || item.IsInterface)
                        continue;
                    if (attr.Interface != null)
                    {
                        if (!attr.Interface.IsInterface)
                            throw new ArgumentException($"指定的服务[{attr.Interface.Name}]类型只能为接口。");
                        if (item.IsAssignableFrom(attr.Interface))
                            throw new ArgumentException($"注入的类型[{item.Name}]未实现指定的服务[{attr.Interface.Name}]。");
                    }
                    if (poolDic.ContainsKey(item))
                        continue;
                    poolDic.Add(item, attr);
                }
            }
            foreach (var item in poolDic)
            {
                switch (item.Value.InjectionType)
                {
                    case InjectionType.Scoped:
                        {
                            if (item.Value.Interface == null)
                                services.TryAddScoped(item.Key);
                            else
                                services.TryAddScoped(item.Value.Interface, item.Key);
                        }
                        break;
                    case InjectionType.Transient:
                        {
                            if (item.Value.Interface == null)
                                services.TryAddTransient(item.Key);
                            else
                                services.TryAddTransient(item.Value.Interface, item.Key);
                        }
                        break;
                    case InjectionType.Singleton:
                        {
                            if (item.Value.Interface == null)
                                services.TryAddSingleton(item.Key);
                            else
                                services.TryAddSingleton(item.Value.Interface, item.Key);
                        }
                        break;
                }
            }
        }
    }
}
