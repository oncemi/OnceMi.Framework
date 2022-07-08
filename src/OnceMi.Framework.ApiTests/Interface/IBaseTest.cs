using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Api;
using System;

namespace OnceMi.Framework.ApiTests.Interface
{
    public class IBaseTest
    {
        public IHost Host { get; private set; }

        public IServiceProvider Services
        {
            get
            {
                return this.Host?.Services;
            }
        }

        public IBaseTest()
        {
            this.Host = Program.CreateHostBuilder(new string[] { }).Build();

            var server = Host.Services.GetService<IServer>();
            var builderFactory = Host.Services.GetService<IApplicationBuilderFactory>();
            var loggerFactory = Host.Services.GetService<ILoggerFactory>();
            var configuration = Host.Services.GetService<IConfiguration>();
            var app =  builderFactory.CreateBuilder(server.Features);
            new Startup(configuration).Configure(app, loggerFactory);
            app.Build();
        }
    }
}
