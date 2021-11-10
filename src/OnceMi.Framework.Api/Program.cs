using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OnceMi.Framework.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();                 //移除已经注册的其他日志处理程序
                    logging.SetMinimumLevel(LogLevel.Trace);  //设置最小的日志级别
                    //logging.AddConsole();
                })
                .UseNLog()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    string baseConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Base.json");
                    if (!File.Exists(baseConfigPath))
                    {
                        throw new Exception($"Base app config not exist. Please check file '{baseConfigPath}'");
                    }
                    config.AddJsonFile(baseConfigPath, optional: false, reloadOnChange: true);

                    string normalConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                    if (File.Exists(normalConfigPath))
                    {
                        config.AddJsonFile(normalConfigPath, optional: false, reloadOnChange: true);
                    }

                    string eventName = hostingContext.HostingEnvironment.EnvironmentName;
                    if (!string.IsNullOrEmpty(eventName))
                    {
                        string eventAppConfigPath = Path.Combine(AppContext.BaseDirectory, $"appsettings.{eventName}.json");
                        if (File.Exists(eventAppConfigPath))
                        {
                            config.AddJsonFile(eventAppConfigPath, optional: false, reloadOnChange: true);
                        }
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
