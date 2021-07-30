using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnceMi.AspNetCore.MQ
{
    static class AssemblyHelper
    {
        /// <summary>
        /// 根据接口获取继承类
        /// </summary>
        /// <param name="interfaceType">继承的接口</param>
        /// <param name="allowInterface">是否包含接口</param>
        /// <returns></returns>
        public static List<Type> GetQueneSubscribes(Type interfaceType, bool allowInterface = false)
        {
            List<Type> result = new List<Type>();
            List<Type> allTypes = GetExportedTypes();
            if (allTypes == null || allTypes.Count == 0)
            {
                return new List<Type>();
            }
            allTypes.ForEach(p =>
            {
                Type[] interfaceTypes = p.GetInterfaces();
                if (interfaceTypes == null || interfaceTypes.Length == 0)
                {
                    return;
                }
                if (allowInterface)
                {
                    if (interfaceTypes.Contains(interfaceType))
                    {
                        result.Add(p);
                    }
                }
                else
                {
                    if (interfaceTypes.Contains(interfaceType) && !p.IsInterface && !p.IsAbstract && p.IsClass)
                    {
                        result.Add(p);
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// 获取当前程序集所有Type
        /// </summary>
        /// <returns></returns>
        private static List<Type> GetExportedTypes()
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

            if (assemblies == null || assemblies.Count == 0)
            {
                throw new Exception("Get assemblies exported types failed.");
            }
            List<Type> result = new List<Type>();
            foreach(var amItem in assemblies)
            {
                try
                {
                    Type[] types = amItem.GetExportedTypes();
                    foreach (var item in types)
                    {
                        if (result.Contains(item))
                        {
                            continue;
                        }
                        result.Add(item);
                    }
                }
                catch (System.TypeLoadException)
                {
                    continue;
                }
                catch
                {
                    throw;
                }
            }
            return result;
        }
    }
}
