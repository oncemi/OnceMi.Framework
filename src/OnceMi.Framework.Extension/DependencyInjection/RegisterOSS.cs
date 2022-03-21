using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnceMi.AspNetCore.OSS;
using OnceMi.Framework.Config;
using OnceMi.Framework.Extension.Middlewares;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterOSS
    {
        public static IServiceCollection AddOSS(this IServiceCollection services)
        {
            OSSConfigNode config = null;
            using (var provider = services.BuildServiceProvider())
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                IConfigurationSection section = configuration.GetSection("OSSProvider");
                if (section == null || !section.Exists())
                {
                    throw new Exception("Can not get oss config from configuration.");
                }
                config = section.Get<OSSConfigNode>();
                if (config == null
                || string.IsNullOrWhiteSpace(config.Endpoint)
                || string.IsNullOrWhiteSpace(config.AccessKey)
                || string.IsNullOrWhiteSpace(config.SecretKey))
                {
                    throw new Exception("Configuration can not bind oss config.");
                }
            }
            //注入OSS缓存
            services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
            // Setup Interception
            services.AddOSSService(option =>
            {
                option.Provider = OSSProvider.Minio;
                option.Endpoint = config.Endpoint;
                option.Region = config.Region;
                option.AccessKey = config.AccessKey;
                option.SecretKey = config.SecretKey;
                option.IsEnableCache = config.IsEnableCache;
                option.IsEnableHttps = config.IsEnableHttps;
            });
            return services;
        }
    }
}
