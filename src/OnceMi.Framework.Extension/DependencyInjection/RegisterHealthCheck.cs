using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using OnceMi.Framework.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OnceMi.Framework.Util.Reflection;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterHealthCheck
    {
        public static IServiceCollection AddHealthCheckService(this IServiceCollection services)
        {
            using (var provider = services.BuildServiceProvider())
            {
                ConfigManager config = provider.GetRequiredService<ConfigManager>();

                #region 注入HealthCheck

                var checksBuilder = services.AddHealthChecks();
                List<Type> hasRegisted = new List<Type>();
                //自动注入实现IHealthCheck的类
                List<Type> allHealthCheckTypes = new AssemblyLoader().GetExportedTypesByInterface(typeof(IHealthCheck));
                foreach (var item in allHealthCheckTypes)
                {
                    checksBuilder.AddCheck(item);
                    hasRegisted.Add(item);
                }

                #endregion

                #region  注入HealthCheck UI

                //get app endpoint
                string host = config.AppSettings.Host;
                string endpoint = config.AppSettings.HealthCheck.HealthCheckEndpoint;
                if (!string.IsNullOrEmpty(host))
                {
                    endpoint = host.EndsWith('/') ? $"{host.TrimEnd('/')}{endpoint}" : $"{host}{endpoint}";
                }

                //Add health checks UI
                services.AddHealthChecksUI(options =>
                {
                    options.UseApiEndpointHttpMessageHandler(sp =>
                    {
                        return new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
                        };
                    });
                    options.AddHealthCheckEndpoint(config.AppSettings.HealthCheck.HealthCheckName, config.AppSettings.HealthCheck.HealthCheckEndpoint);
                    options.SetEvaluationTimeInSeconds(config.AppSettings.HealthCheck.EvaluationTimeinSeconds);
                    options.SetMinimumSecondsBetweenFailureNotifications(config.AppSettings.HealthCheck.MinimumSecondsBetweenFailureNotifications);
                    options.MaximumHistoryEntriesPerEndpoint(config.AppSettings.HealthCheck.MaximumHistoryEntriesPerEndpoint);
                })
                    .AddInMemoryStorage();

                #endregion

                return services;
            }
        }

        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
        {
            ConfigManager config = app.ApplicationServices.GetRequiredService<ConfigManager>();

            app.UseHealthChecks(config.AppSettings.HealthCheck.HealthCheckEndpoint, new HealthCheckOptions()
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
            //    options.UIPath = config.AppSettings.HealthCheck.HealthCheckUIPath;
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
