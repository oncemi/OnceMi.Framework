using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using OnceMi.Framework.Config;

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
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    ConfigManager.LoadAppsettings(hostingContext, configuration);
                })
                .ConfigureWebHostDefaults(host =>
                {
                    host.UseStartup<Startup>();
                });
    }
}
