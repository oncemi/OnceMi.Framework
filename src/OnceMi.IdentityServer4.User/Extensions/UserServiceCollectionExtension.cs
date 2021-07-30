using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.IdentityServer4.User
{
    public static class UserServiceCollectionExtension
    {
        public static IServiceCollection AddUserDbContext(this IServiceCollection services
            , IFreeSql<UserDbContext> freeSql)
        {
            services.AddSingleton(freeSql);
            services.AddScoped<UserDbContext>();
            return services;
        }

        public static IServiceCollection AddUserDbContext(this IServiceCollection services
            , FreeSqlBuilder freeSqlBuilder)
        {
            var freeSql = freeSqlBuilder.Build<UserDbContext>(); ;

            services.AddSingleton(freeSql);
            services.AddScoped<UserDbContext>();
            return services;
        }

    }
}
