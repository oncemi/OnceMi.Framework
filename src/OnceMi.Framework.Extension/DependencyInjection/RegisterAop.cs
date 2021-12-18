using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using OnceMi.Framework.Extension.Aop;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterAop
    {
        public static IServiceCollection AddAop(this IServiceCollection services)
        {
            // Setup Interception
            services.AddSingleton(new ProxyGenerator());
            services.AddScoped<ICleanCacheAsyncInterceptor, CleanCacheAsyncInterceptor>();
            services.AddScoped<ITransactionAsyncInterceptor, TransactionAsyncInterceptor>();

            return services;
        }
    }
}
