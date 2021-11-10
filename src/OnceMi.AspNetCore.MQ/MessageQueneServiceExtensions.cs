using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnceMi.AspNetCore.MQ;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnceMi.AspNetCore.MQ
{
    public static class MessageQueneServiceExtensions
    {
        public static IServiceCollection AddMessageQuene(this IServiceCollection services, Action<MqOptions> options)
        {
            services.Configure(MqOptionSetting.Name, options);
            services.TryAddSingleton<IMessageQueneService, MessageQueneService>();

            //自动注入实现了接口了ISubscribe类
            List<Type> serviceTypes = AssemblyHelper.GetQueneSubscribes(typeof(ISubscribe));
            if (serviceTypes != null && serviceTypes.Count > 0)
            {
                foreach (var item in serviceTypes)
                {
                    services.TryAddSingleton(item);
                }
            }
            return services;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMessageQuene(this IApplicationBuilder app)
        {
            List<Type> serviceTypes = AssemblyHelper.GetQueneSubscribes(typeof(ISubscribe));
            if (serviceTypes == null || serviceTypes.Count == 0)
            {
                return app;
            }
            foreach (var item in serviceTypes)
            {
                var services = app.ApplicationServices.GetServices(item);
                if (services == null || services.Count() == 0)
                {
                    continue;
                }
                foreach (var serviceItem in services)
                {
                    ISubscribe sub = (ISubscribe)serviceItem;
                    sub?.Excute();
                }
            }
            return app;
        }
    }
}
