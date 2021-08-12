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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterHealthCheckService
    {
        public static IServiceCollection AddHealthCheckService(this IServiceCollection services)
        {
            using (var provider = services.BuildServiceProvider())
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                IServer server = provider.GetRequiredService<IServer>();

                var checksBuilder = services.AddHealthChecks();
                List<Type> hasRegisted = new List<Type>();
                //自动注入实现IHealthCheck的类
                List<Type> allHealthCheckTypes = new AssemblyLoader().GetExportedTypesByInterface(typeof(IHealthCheck));
                foreach (var item in allHealthCheckTypes)
                {
                    checksBuilder.AddCheck(item);
                    hasRegisted.Add(item);
                }
                //get app endpoint
                string endpoint = configuration.GetValue<string>("AppSettings:Host");
                HealthCheckNode config = configuration.GetSection("AppSettings:HealthCheck").Get<HealthCheckNode>();
                if (config == null || string.IsNullOrEmpty(config.HealthCheckUIPath))
                {
                    throw new Exception("Configuration can not bind healthcheck config.");
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
                    options.AddHealthCheckEndpoint(config.HealthCheckName, $"{endpoint}{(config.HealthCheckUIPath.StartsWith('/') ? config.HealthCheckUIPath : "/" + config.HealthCheckUIPath)}");
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
            IConfiguration configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            string healthCheckUIPath = configuration.GetValue<string>("AppSettings:HealthCheck:HealthCheckUIPath");
            if (!healthCheckUIPath.StartsWith('/'))
            {
                healthCheckUIPath = "/" + healthCheckUIPath;
            }

            app.UseHealthChecks(healthCheckUIPath, new HealthCheckOptions()
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

        private static string GetListenAddress(IConfiguration configuration, IServer server)
        {
            string endpoint = null;
            var address = server.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (address == null || address.Length == 0)
            {
                throw new Exception("Can not get current app endpoint.");
            }
            if (address.Length > 1)
            {
                foreach (var item in address)
                {
                    if (item.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        endpoint = item;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(endpoint))
                {
                    endpoint = address[0];
                }
            }
            else
            {
                endpoint = address[0];
            }
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception("Can not get current app endpoint.");
            }
            return endpoint.Replace("[::]", "localhost").Replace("0.0.0.0", "localhost");
        }
    }
}
