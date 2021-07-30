using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.AspNetCore.MQ;
using OnceMi.AspNetCore.OSS;
using OnceMi.Framework.Api.Middlewares;
using OnceMi.Framework.Config;
using OnceMi.Framework.DependencyInjection;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.Extension.Filters;
using OnceMi.Framework.Extension.Helpers;
using OnceMi.Framework.Extension.Middlewares;
using OnceMi.Framework.Model;
using OnceMi.Framework.Util.Json;
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnceMi.Framework.Api
{
    public class Startup
    {
        private const string _defaultOrigins = "DefaultCorsPolicy";
        //全局json对象返回设置，默认小驼峰
        private JsonNamingPolicy _jsonNamingPolicy = JsonNamingPolicy.CamelCase;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            #region IdGenerator

            services.AddIdGenerator(x =>
            {
                x.AppId = Configuration.GetValue<ushort>("AppSettings:AppId");
            });

            #endregion

            //MemoryCache
            services.AddMemoryCache();
            //ConfigManager
            services.AddConfig();
            //Db
            services.AddDatabase();
            //AutoMapper
            services.AddMapper();
            //RedisCahe
            services.AddRedisCache();
            //Swagger
            services.AddSwagger();

            #region OSS

            services.AddOSSService(option =>
            {
                OSSConfigNode config = Configuration.GetSection("OSSProvider").Get<OSSConfigNode>();
                if (config == null
                || string.IsNullOrWhiteSpace(config.Endpoint)
                || string.IsNullOrWhiteSpace(config.AccessKey)
                || string.IsNullOrWhiteSpace(config.SecretKey)
                || string.IsNullOrWhiteSpace(config.DefaultBucketName))
                {
                    throw new Exception("Configuration can not bind oss config.");
                }
                option.Provider = OSSProvider.Minio;
                option.Endpoint = config.Endpoint;
                option.Region = config.Region;
                option.AccessKey = config.AccessKey;
                option.SecretKey = config.SecretKey;
                option.IsEnableCache = config.IsEnableCache;
            });

            #endregion

            #region Service & Repository

            services.AddRepository();
            services.AddService();

            #endregion

            #region 跨域

            services.AddCors(options =>
            {
                options.AddPolicy(_defaultOrigins, policy =>
                 {
                     policy.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowAnyOrigin();
                 });
            });

            #endregion

            #region HealthCheck

            services.AddHealthCheckService();

            #endregion

            #region 消息队列

            services.AddMessageQuene(option =>
            {
                option.UseExternalRedisClient = true;
                option.AppId = Configuration.GetValue<int>("AppSettings:AppId");
                option.ProviderType = Configuration.GetValue<MqProviderType>("MessageQueneSetting:ProviderType");
                option.Connectstring = Configuration.GetValue<string>("MessageQueneSetting:ConnectionString");
            });

            #endregion

            #region Aop

            services.AddAop();

            #endregion

            #region 认证与授权

            //Json序列化处理
            services.Configure<CustumJsonSerializerOptions>(option =>
            {
                option.JsonNamingPolicy = _jsonNamingPolicy;
            });
            var token = Configuration.GetSection("TokenManagement").Get<TokenManagementNode>();

            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(x =>
                {
                    x.Authority = Configuration.GetValue<string>("IdentityServer:Url");
                    x.Audience = Configuration.GetValue<string>("IdentityServer:Audience");
                    x.RequireHttpsMetadata = Configuration.GetValue<bool>("IdentityServer:RequireHttps");
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        //RoleClaimType = ClaimTypes.Role,
                        NameClaimType = JwtClaimTypes.Name,

                        RequireExpirationTime = true, //过期时间
                        ClockSkew = TimeSpan.FromMinutes(5),
                    };
                    x.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            if (!string.IsNullOrEmpty(context.ErrorDescription))
                                await WriteResponse(context.Response, StatusCodes.Status401Unauthorized, context.ErrorDescription);
                            else
                                await WriteResponse(context.Response, StatusCodes.Status401Unauthorized, "Unauthorized");
                            context.HandleResponse();
                        },
                        OnForbidden = async context =>
                        {
                            await WriteResponse(context.Response, StatusCodes.Status403Forbidden, "Forbidden");
                        },
                        OnAuthenticationFailed = context =>
                        {
                            // 如果过期，则把<是否过期>添加到，返回头信息中
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                //全局用户授权
                options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            #endregion

            #region Quartz定时任务

            services.AddQuartz();

            #endregion

            #region Controller

            services.AddHttpContextAccessor();
            services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);
            services.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
            services.AddHostedService<LifetimeEventsService>();
            services.Configure<HostOptions>(option =>
            {
                option.ShutdownTimeout = TimeSpan.FromSeconds(10);
            });

            services.AddControllers(options =>
            {
                //全局异常
                options.Filters.Add(typeof(GlobalExceptionFilter));
                //封装返回数据格式
                options.Filters.Add(typeof(GlobalApiResponseFilter));
                //全局授权过滤器
                options.Filters.Add(typeof(GlobalPermissionFilter));
                //重复请求过滤器 未完成
                //options.Filters.Add(typeof(GolbalTranActionFilter));
            })
                .AddJsonOptions(options =>
                {
                    //中文等特殊字符序列化
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                    //包含公共字段
                    options.JsonSerializerOptions.IncludeFields = true;
                    //忽略大小写
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    //DateTime
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                    options.JsonSerializerOptions.Converters.Add(new DateTimeNullableConverter());
                    //小驼峰
                    options.JsonSerializerOptions.PropertyNamingPolicy = _jsonNamingPolicy;
                });

            //ApiBehaviorOptions必须在AddControllers之后
            services.Configure<ApiBehaviorOptions>(options =>
            {
                //重写模型验证返回数据格式
                options.InvalidModelStateResponseFactory = RewriteHelper.RewriteInvalidModelStateResponse;
            });

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app
            , IWebHostEnvironment env
            , ILoggerFactory loggerFactory)
        {
            #region 全局异常处理

            //此次注册的是全局异常处理，用于捕获Filter中无法捕获的异常（内部异常）
            //所以需要异常处理方法应该实现修改Filter中异常处理，当Filter中异常处理中，不会再次进入此处处理
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    await RewriteHelper.GlobalExceptionHandler(context, loggerFactory, _jsonNamingPolicy);
                });
            });

            #endregion

            //配置文件
            app.UseConfig();
            //Swagger
            app.UseSwaggerWithUI();
            //初始化数据库
            app.UseDbSeed();
            //健康检查
            app.UseHealthChecks();
            //跨域
            app.UseCors(_defaultOrigins);
            //RabbitMQ消息队列
            app.UseMessageQuene();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //写请求日志
            app.UseRequestLogging();

            app.UseEndpoints(endpoints =>
            {
                //MapHealthChecksUI应该统一写到UseHealthChecks中
                //但是有bug，具体请看UseHealthChecks中注释
                //issue：https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/716
                endpoints.MapHealthChecksUI(options =>
                {
                    options.UseRelativeResourcesPath = false;
                    options.UseRelativeApiPath = false;
                    options.UseRelativeWebhookPath = false;
                    options.UIPath = "/sys/health-ui";
                }).AllowAnonymous();

                endpoints.MapControllers();
            });
        }

        private async Task WriteResponse(HttpResponse response, int statusCode, string message)
        {
            response.ContentType = "application/json";
            response.StatusCode = statusCode;

            await response.WriteAsync(JsonUtil.SerializeToString(new ResultObject<object>()
            {
                Code = statusCode,
                Message = message,
            }, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = _jsonNamingPolicy
            }));
        }
    }
}
