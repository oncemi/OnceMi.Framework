using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OnceMi.Framework.Extension.Injection
{
    /// <summary>
    /// 注入AOP代理扩展
    /// </summary>
    internal static class ProxiedServicesExtensions
    {
        #region AddScoped

        public static void AddScopedWithProxied<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddScoped<TImplementation>();
            services.AddScoped(typeof(TInterface), provider => BuildProxyTarget(services, provider, typeof(TInterface), typeof(TImplementation)));
        }

        public static void AddScopedWithProxied(this IServiceCollection services, Type interfaceType, Type implementationType)
        {
            services.AddScoped(implementationType);
            services.AddScoped(interfaceType, provider => BuildProxyTarget(services, provider, interfaceType, implementationType));
        }

        public static void TryAddScopedWithProxied<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.TryAddScoped<TImplementation>();
            services.TryAddScoped(typeof(TInterface), provider => BuildProxyTarget(services, provider, typeof(TInterface), typeof(TImplementation)));
        }

        public static void TryAddScopedWithProxied(this IServiceCollection services, Type interfaceType, Type implementationType)
        {
            services.TryAddScoped(implementationType);
            services.TryAddScoped(interfaceType, provider => BuildProxyTarget(services, provider, interfaceType, implementationType));
        }

        #endregion

        #region AddTransient

        public static void AddTransientWithProxied<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddScoped<TImplementation>();
            services.AddScoped(typeof(TInterface), provider => BuildProxyTarget(services, provider, typeof(TInterface), typeof(TImplementation)));
        }

        public static void AddTransientWithProxied(this IServiceCollection services, Type interfaceType, Type implementationType)
        {
            services.AddScoped(implementationType);
            services.AddScoped(interfaceType, provider => BuildProxyTarget(services, provider, interfaceType, implementationType));
        }

        public static void TryAddTransientWithProxied<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.TryAddScoped<TImplementation>();
            services.TryAddScoped(typeof(TInterface), provider => BuildProxyTarget(services, provider, typeof(TInterface), typeof(TImplementation)));
        }

        public static void TryAddTransientWithProxied(this IServiceCollection services, Type interfaceType, Type implementationType)
        {
            services.TryAddScoped(implementationType);
            services.TryAddScoped(interfaceType, provider => BuildProxyTarget(services, provider, interfaceType, implementationType));
        }

        #endregion

        #region AddSingleton

        public static void AddSingletonWithProxied<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddScoped<TImplementation>();
            services.AddScoped(typeof(TInterface), provider => BuildProxyTarget(services, provider, typeof(TInterface), typeof(TImplementation)));
        }

        public static void AddSingletonWithProxied(this IServiceCollection services, Type interfaceType, Type implementationType)
        {
            services.AddScoped(implementationType);
            services.AddScoped(interfaceType, provider => BuildProxyTarget(services, provider, interfaceType, implementationType));
        }

        public static void TryAddSingletonWithProxied<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.TryAddScoped<TImplementation>();
            services.TryAddScoped(typeof(TInterface), provider => BuildProxyTarget(services, provider, typeof(TInterface), typeof(TImplementation)));
        }

        public static void TryAddSingletonWithProxied(this IServiceCollection services, Type interfaceType, Type implementationType)
        {
            services.TryAddScoped(implementationType);
            services.TryAddScoped(interfaceType, provider => BuildProxyTarget(services, provider, interfaceType, implementationType));
        }

        #endregion

        private static object BuildProxyTarget(IServiceCollection services, IServiceProvider provider, Type interfaceType, Type implementationType)
        {
            var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
            var actual = provider.GetRequiredService(implementationType);
            var interceptorServiceList = services
                .Where(p => p.ServiceType != null 
                    && p.ImplementationType != null 
                    && p.ServiceType.IsInterface 
                    && p.ServiceType.GetInterfaces()?.Any(q => q == typeof(IAsyncInterceptor)) == true)
                .ToList();
            if (interceptorServiceList == null || interceptorServiceList.Count == 0)
            {
                return actual;
            }
            IAsyncInterceptor[] interceptors = new IAsyncInterceptor[interceptorServiceList.Count];
            for (int i = 0; i < interceptorServiceList.Count; i++)
            {
                var obj = provider.GetService(interceptorServiceList[i].ServiceType);
                interceptors[i] = obj as IAsyncInterceptor;
            }
            return proxyGenerator.CreateInterfaceProxyWithTarget(interfaceType, actual, interceptors);
        }
    }
}
