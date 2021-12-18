using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OnceMi.AspNetCore.AutoInjection;
using OnceMi.Framework.Config;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Extensions;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterAutoMapper
    {
        public static IServiceCollection AddMapper<T>(this IServiceCollection services) where T : IProfileConfiguration
        {
            services.AddAutoMapper(typeof(T));
            return services;
        }

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            //自动注入Profile
            List<Type> profileTypes = new AssemblyLoader(p => p.Name.StartsWith(GlobalConfigConstant.FirstNamespace, StringComparison.OrdinalIgnoreCase))
                .GetExportedTypesByBaseType(typeof(Profile));
            List<MapperEntityConfig> mapperList = GetMapperFrom();
            List<MapperEntityConfig> mapperToList = GetMapperTo();
            foreach (var item in mapperToList)
            {
                if (mapperList.Any(p => p.Source == item.Source && p.Dest == item.Dest)) continue;
                mapperList.Add(item);
            }

            using (var provider = services.BuildServiceProvider())
            {
                IWebHostEnvironment env = provider.GetRequiredService<IWebHostEnvironment>();
                var config = new MapperConfiguration(cfg =>
                {
                    if (profileTypes != null && profileTypes.Count > 0)
                    {
                        var profiles = profileTypes.Select(p => Activator.CreateInstance(p)).Cast<Profile>().ToList();
                        cfg.AddProfiles(profiles);
                    }
                    if (mapperList != null && mapperList.Count > 0)
                    {
                        foreach (var item in mapperList)
                        {
                            cfg.CreateMap(item.Source, item.Dest);
                        }
                    }
                    //禁止有相同类型的map
                    cfg.Advanced.AllowAdditiveTypeMapCreation = false;
                    //忽略未映射的属性
                    cfg.IgnoreUnmapped();
                });

                if (env.IsDevelopment())
                {
                    //开发环境下，检查Automapper配置是否正确
                    config.AssertConfigurationIsValid();
                }
                var mapper = config.CreateMapper();
                services.TryAddSingleton(mapper);
                return services;
            }
        }

        #region Private

        /// <summary>
        /// 自动注入包含MapperFromAttribute的类
        /// </summary>
        /// <returns></returns>
        private static List<MapperEntityConfig> GetMapperFrom()
        {
            List<Type> types = new AssemblyLoader().GetExportedTypesByAttribute(typeof(MapperFromAttribute));
            if (types == null || types.Count == 0)
                return new List<MapperEntityConfig>();
            List<MapperEntityConfig> result = new List<MapperEntityConfig>();
            foreach (var item in types)
            {
                var attributes = item.GetCustomAttributes(typeof(MapperFromAttribute), false);
                if (attributes == null || attributes.Length == 0)
                {
                    continue;
                }
                foreach (var attrItem in attributes)
                {
                    Type mapType = ((MapperFromAttribute)attrItem).MapperType;
                    if (mapType == null)
                    {
                        continue;
                    }
                    if (!result.Any(p => p.Source == mapType && p.Dest == item))
                    {
                        result.Add(new MapperEntityConfig()
                        {
                            Source = mapType,
                            Dest = item
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 自动注入包含MapperToAttribute的类
        /// </summary>
        /// <returns></returns>
        private static List<MapperEntityConfig> GetMapperTo()
        {
            List<Type> types = new AssemblyLoader().GetExportedTypesByAttribute(typeof(MapperToAttribute));
            if (types == null || types.Count == 0)
                return new List<MapperEntityConfig>();
            List<MapperEntityConfig> result = new List<MapperEntityConfig>();
            foreach (var item in types)
            {
                var attributes = item.GetCustomAttributes(typeof(MapperToAttribute), false);
                if (attributes == null || attributes.Length == 0)
                {
                    continue;
                }
                foreach (var attrItem in attributes)
                {
                    Type mapType = ((MapperToAttribute)attrItem).MapperType;
                    if (mapType == null)
                    {
                        continue;
                    }
                    if (!result.Any(p => p.Source == item && p.Dest == mapType))
                    {
                        result.Add(new MapperEntityConfig()
                        {
                            Source = item,
                            Dest = mapType
                        });
                    }
                }
            }
            return result;
        }

        private class MapperEntityConfig
        {
            public Type Source { get; set; }

            public Type Dest { get; set; }
        }

        #endregion
    }
}
