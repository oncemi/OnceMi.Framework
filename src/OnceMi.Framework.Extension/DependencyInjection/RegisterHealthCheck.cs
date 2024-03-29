﻿using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OnceMi.AspNetCore.AutoInjection;
using OnceMi.Framework.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

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
                List<Type> allHealthCheckTypes = new AssemblyLoader(p => p.Name.StartsWith(GlobalConfigConstant.FirstNamespace, StringComparison.OrdinalIgnoreCase))
                    .GetExportedTypesByInterface(typeof(IHealthCheck));
                foreach (var item in allHealthCheckTypes)
                {
                    checksBuilder.AddCheck(item);
                    hasRegisted.Add(item);
                }

                #endregion

                #region  注入HealthCheck UI

                //是否开启UI
                if (config.AppSettings.HealthCheck.IsEnabledHealthCheckUI)
                {
                    //get app endpoint
                    string host = config.AppSettings.Host;
                    string endpoint = config.AppSettings.HealthCheck.HealthCheckEndpoint;
                    if (!string.IsNullOrEmpty(host))
                    {
                        endpoint = MapHealthcheckEndpoint(host, endpoint);
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
                        /*
                         * 异常：IPv4 address 0.0.0.0 and IPv6 address ::0 are unspecified addresses that cannot be used as a target address. 
                         * Issue：https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/410
                         * 使用绝对地址在本地使用localhost调试时没有问题，但是发布后无法请求0.0.0.0。
                         * 所以此处修改为配置文件中host不为空或者非调试环境下，使用完整url
                         * 在上面的issue里面已经提出了解决访问，但是作者好想并没有修改，也不晓得为啥
                         */
                        options.AddHealthCheckEndpoint(config.AppSettings.HealthCheck.HealthCheckName, endpoint);
                        options.SetEvaluationTimeInSeconds(config.AppSettings.HealthCheck.EvaluationTimeinSeconds);
                        options.SetMinimumSecondsBetweenFailureNotifications(config.AppSettings.HealthCheck.MinimumSecondsBetweenFailureNotifications);
                        options.MaximumHistoryEntriesPerEndpoint(config.AppSettings.HealthCheck.MaximumHistoryEntriesPerEndpoint);
                    })
                        .AddInMemoryStorage();
                }
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

        public static IEndpointRouteBuilder MapHealthChecksUI(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("AppSettings:HealthCheck:IsEnabledHealthCheckUI"))
            {
                //MapHealthChecksUI应该统一写到UseHealthChecks中
                //但是有bug，具体请看UseHealthChecks中注释
                //issue：https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/716
                endpoints.MapHealthChecksUI(options =>
                {
                    options.UseRelativeResourcesPath = false;
                    options.UseRelativeApiPath = false;
                    options.UseRelativeWebhookPath = false;
                    options.UIPath = configuration.GetValue<string>("AppSettings:HealthCheck:HealthCheckUIPath");
                }).AllowAnonymous();
            }
            return endpoints;
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

        private static string MapHealthcheckEndpoint(string host, string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception("Health check endpoint can not null");
            }
            if (!endpoint.StartsWith('/'))
            {
                endpoint = "/" + endpoint;
            }
            if (string.IsNullOrEmpty(host))
            {
                return endpoint;
            }
            var uri = Regex.Replace(host.TrimEnd('/'), @"^(?<scheme>https?):\/\/((\+)|(\*)|(0.0.0.0))(?=[\:\/]|$)", "${scheme}://localhost");
            Uri httpEndpoint = new Uri(uri, UriKind.Absolute);
            return new UriBuilder(httpEndpoint.Scheme, httpEndpoint.Host, httpEndpoint.Port, endpoint).ToString();
        }
    }
}
