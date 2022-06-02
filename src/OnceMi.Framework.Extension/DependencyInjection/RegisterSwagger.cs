using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OnceMi.Framework.Extension.Filters;
using OnceMi.Framework.Extension.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterSwagger
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddSwaggerGen(option =>
            {
                //获取全部的api verison
                List<ApiVersion> apiVersions = ApiVersionHelper.GetAllApiVersions();
                if (apiVersions == null || apiVersions.Count == 0)
                {
                    throw new Exception("Can not get api version from this app.");
                }
                //注册api verison
                foreach (var item in apiVersions)
                {
                    option.SwaggerDoc($"v{item}", new OpenApiInfo
                    {
                        Version = $"v{item}",
                        Title = $" OnceMi.Framework API V{item}",
                        Description = "API for OnceMi.Framework",
                        Contact = new OpenApiContact()
                        {
                            Name = "OnceMi",
                            Email = "open@oncemi.com"
                        }
                    });
                }
                option.CustomOperationIds(api =>
                {
                    var descriptor = api.ActionDescriptor as ControllerActionDescriptor;
                    return descriptor.ControllerName + "-" + descriptor.ActionName;
                });
                option.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var versions = apiDesc.CustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);
                    return versions.Any(item => $"v{item}" == docName);
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 在下方输入Bearer {token} 即可，注意两者之间有空格。",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                });

                option.OperationFilter<SwaggerParameterOperationFilter>();
                option.DocumentFilter<SwaggerVersionDocumentFilter>();

                //项目xml文档
                string[] files = Directory.GetFiles(AppContext.BaseDirectory);
                Array.ForEach(files, p =>
                {
                    if (p.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) && File.Exists(p.Substring(0, p.Length - 4) + ".dll"))
                    {
                        option.IncludeXmlComments(p, true);
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app)
        {
            app.UseSwagger();

            //默认的UI
            app.UseSwaggerUI(option =>
            {
                List<ApiVersion> apiVersions = ApiVersionHelper.GetAllApiVersions();
                foreach (var item in apiVersions)
                {
                    option.SwaggerEndpoint($"/swagger/v{item}/swagger.json", $"V{item} Docs");
                }
                option.RoutePrefix = "sys/swagger-ui";
                option.DocumentTitle = "OnceMi Framework Api Document";
            });

            //Knife4UI
            //app.UseKnife4UI(option =>
            //{
            //    List<ApiVersion> apiVersions = ApiVersionHelper.GetAllApiVersions();
            //    foreach (var item in apiVersions)
            //    {
            //        option.SwaggerEndpoint($"/swagger/v{item}/swagger.json", $"V{item} Docs");
            //    }
            //    option.RoutePrefix = "sys/swagger-ui";
            //    option.DocumentTitle = "OnceMi Framework Api Document";
            //});

            return app;
        }
    }
}
