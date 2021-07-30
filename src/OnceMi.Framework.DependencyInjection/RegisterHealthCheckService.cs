using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using OnceMi.Framework.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OnceMi.Framework.Extension.HealthCheck;
using OnceMi.Framework.Util.Reflection;

namespace OnceMi.Framework.DependencyInjection
{
    public static class RegisterHealthCheckService
    {
        public static IServiceCollection AddHealthCheckService(this IServiceCollection services)
        {
            using (var provider = services.BuildServiceProvider())
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                var checksBuilder = services.AddHealthChecks();
                List<Type> hasRegisted = new List<Type>();
                //自动注入实现IHealthCheck的类
                List<Type> allHealthCheckTypes = new AssemblyHelper().GetExportedTypesByInterface(typeof(IHealthCheck));
                foreach(var item in allHealthCheckTypes)
                {
                    checksBuilder.AddCheck(item);
                    hasRegisted.Add(item);
                }
                //Add health checks UI
                services.AddHealthChecksUI(options =>
                {
                    HealthCheckNode config = configuration.GetSection("AppSettings:HealthCheck").Get<HealthCheckNode>();
                    if (config == null || string.IsNullOrEmpty(config.HealthCheckEndpoint))
                    {
                        throw new Exception("Configuration can not bind healthcheck config.");
                    }
                    options.AddHealthCheckEndpoint(config.HealthCheckName, config.HealthCheckEndpoint);
                    options.SetEvaluationTimeInSeconds(config.EvaluationTimeinSeconds);
                    options.SetMinimumSecondsBetweenFailureNotifications(config.MinimumSecondsBetweenFailureNotifications);
                    options.MaximumHistoryEntriesPerEndpoint(config.MaximumHistoryEntriesPerEndpoint);
                })
                    .AddInMemoryStorage();
                return services;
            }
        }

        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/sys/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            //有bug，工作不正常，界面上不显示header text和刷新时间
            //所以只能配置到UseEndpoints中
            //app.UseHealthChecksUI(options =>
            //{
            //    options.UseRelativeResourcesPath = false;
            //    options.UseRelativeApiPath = false;
            //    options.UseRelativeWebhookPath = false;
            //    options.UIPath = "/sys/health-ui";
            //});
            return app;
        }

        private static IHealthChecksBuilder AddCheck(this IHealthChecksBuilder builder, Type type)
        {
            builder.Services.AddTransient(type);
            using (var provider = builder.Services.BuildServiceProvider())
            {
                IEnumerable<object> result = provider.GetServices(type);
                if (result == null || !result.Any())
                {
                    return builder;
                }
                object obj = result.First();
                if (obj != null && obj is IHealthCheck resultObj)
                {
                    builder.AddCheck(type.Name, resultObj);
                }
            }
            return builder;
        }
    }
}
