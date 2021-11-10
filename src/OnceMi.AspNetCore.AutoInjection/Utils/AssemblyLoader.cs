using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnceMi.AspNetCore.AutoInjection
{
    public sealed class AssemblyLoader
    {
        public List<Type> DomainAllTypes { get; set; }

        public List<Assembly> DomainAllAssemblies { get; set; }

        private readonly string[] _ignoreNamespaces = { "dotnet-"
            , "Microsoft."
            , "mscorlib"
            , "netstandard"
            , "System"
            , "Windows"
            , "Castle"
            , "AutoMapper"
            , "Quartz"
            , "Swashbuckle"
            , "Newtonsoft.Json"
            , "NLog"
            , "FreeSql"
            , "HealthChecks"
        };

        private readonly Func<AssemblyName, bool> _filter = null;

        public AssemblyLoader(Func<AssemblyName, bool> filter = null)
        {
            if (filter != null)
                _filter = p => p.Name != null && !_ignoreNamespaces.Any(m => p.Name.StartsWith(m)) && filter(p);
            else
                _filter = p => p.Name != null && !_ignoreNamespaces.Any(m => p.Name.StartsWith(m));

            this.DomainAllAssemblies = this.GetDomainAssemblies();
            this.DomainAllTypes = this.GetExportedTypes();
        }

        /// <summary>
        /// 获取当前程序集所有Type
        /// </summary>
        /// <returns></returns>
        public List<Type> GetExportedTypes()
        {
            if (this.DomainAllTypes != null && this.DomainAllTypes.Count > 0)
            {
                return this.DomainAllTypes;
            }
            if (this.DomainAllAssemblies == null || this.DomainAllAssemblies.Count == 0)
            {
                throw new Exception("Get assemblies exported types failed.");
            }
            List<Type> result = new List<Type>();
            this.DomainAllAssemblies.ForEach(p =>
            {
                Type[] types = p.GetExportedTypes();
                foreach (var item in types)
                {
                    if (result.Contains(item))
                    {
                        continue;
                    }
                    result.Add(item);
                }
            });
            return result;
        }

        /// <summary>
        /// 根据接口获取继承类
        /// </summary>
        /// <param name="interfaceType">继承的接口</param>
        /// <param name="allowInterface">是否包含接口</param>
        /// <returns></returns>
        public List<Type> GetExportedTypesByInterface(Type interfaceType, bool allowInterface = false)
        {
            List<Type> result = new List<Type>();
            if (this.DomainAllTypes == null || this.DomainAllTypes.Count == 0)
            {
                return new List<Type>();
            }
            this.DomainAllTypes.ForEach(p =>
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
        /// 根据基类获取Type
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public List<Type> GetExportedTypesByBaseType(Type baseType)
        {
            List<Type> result = new List<Type>();
            if (this.DomainAllTypes == null || this.DomainAllTypes.Count == 0)
            {
                return new List<Type>();
            }
            this.DomainAllTypes.ForEach(p =>
            {
                Type itemBaseType = p.BaseType;
                if (itemBaseType == null || itemBaseType != baseType)
                {
                    return;
                }
                if (!p.IsInterface && !p.IsAbstract && p.IsClass)
                {
                    result.Add(p);
                }
            });
            return result;
        }

        /// <summary>
        /// 根据类型继承接口中的fullName反射获取Type
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public List<Type> GetExportedTypesByInterfaceFullName(string fullName)
        {
            List<Type> result = new List<Type>();
            if (this.DomainAllTypes == null || this.DomainAllTypes.Count == 0)
            {
                return new List<Type>();
            }
            foreach (var item in this.DomainAllTypes)
            {
                if (!item.IsClass || !item.IsPublic || item.IsAbstract || item.IsInterface)
                {
                    continue;
                }
                Type[] interfaces = item.GetInterfaces();
                if (interfaces == null || interfaces.Length == 0)
                {
                    continue;
                }
                Type resultItem = interfaces.Where(q => !string.IsNullOrEmpty(q.FullName) && q.FullName.StartsWith(fullName)).FirstOrDefault();
                if (resultItem != null)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据类型继承接口中的Type的fullName反射获取Type
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public List<Type> GetExportedTypesByInterfaceFullName(Type mapperFromInterfaceType)
        {
            if (mapperFromInterfaceType == null || string.IsNullOrEmpty(mapperFromInterfaceType.FullName))
            {
                return new List<Type>();
            }
            string fullName = mapperFromInterfaceType.FullName?.Split('`')?.Length > 1 ? mapperFromInterfaceType.FullName?.Split('`')[0] : null;
            if (string.IsNullOrEmpty(fullName))
            {
                return new List<Type>();
            }
            return GetExportedTypesByInterfaceFullName(fullName);
        }

        /// <summary>
        /// 从Attribute获取Type
        /// </summary>
        /// <param name="attrType"></param>
        /// <returns></returns>
        public List<Type> GetExportedTypesByAttribute(Type attrType)
        {
            Type baseType = attrType.BaseType;
            if (baseType == null || baseType != typeof(System.Attribute))
            {
                throw new Exception("The input type is not attribute");
            }
            List<Type> types = this.DomainAllTypes
                ?.Where(p =>
                {
                    var attrs = p.GetCustomAttributes();
                    if (attrs == null || attrs.Count() == 0) return false;
                    if (attrs.Any(q => q.GetType() == attrType)) return true;
                    return false;
                })
                .ToList();
            if (types == null)
                return new List<Type>();
            return types;
        }

        /// <summary>
        /// 获取接口和继承接口的类
        /// </summary>
        /// <param name="interfaceBaseType"></param>
        /// <param name="baseEntityType"></param>
        /// <returns></returns>
        public Dictionary<Type, Type> GetInheritInterfaceTypes(Type interfaceBaseType)
        {
            if (!interfaceBaseType.IsInterface)
            {
                throw new Exception("Only allow interface");
            }
            Dictionary<Type, Type> registerDic = new Dictionary<Type, Type>();
            //程序集所有继承了接口的类
            List<Type> allBusInterfaceTypes = GetExportedTypesByInterface(interfaceBaseType, true)
                ?.Where(p => p.IsInterface
                    && !string.IsNullOrEmpty(p.FullName)
                    && p != interfaceBaseType
                    && p.GetCustomAttribute<IgnoreDependencyAttribute>() == null)
                ?.ToList();
            if (allBusInterfaceTypes == null || allBusInterfaceTypes.Count == 0)
            {
                return registerDic;
            }

            foreach (var item in allBusInterfaceTypes)
            {
                List<Type> busTypes = GetExportedTypesByInterface(item);
                if (busTypes == null || busTypes.Count != 1)
                {
                    continue;
                }
                registerDic.Add(item, busTypes[0]);
            }
            return registerDic;
        }

        #region private

        /// <summary>
        /// 获取当前项目引用的所有程序集
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private List<Assembly> GetDomainAssemblies(Func<AssemblyName, bool> filter = null)
        {
            List<Assembly> assemblies = DependencyContext.Default.GetDefaultAssemblyNames()
                .Where(_filter)
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
            return assemblies.ToList();
        }
        #endregion
    }
}
