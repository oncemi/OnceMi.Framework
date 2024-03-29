using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Api.Middlewares;
using OnceMi.Framework.Config;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.Extension.DependencyInjection;
using OnceMi.Framework.Extension.Filters;
using OnceMi.Framework.Extension.Helpers;
using OnceMi.Framework.Util.Json;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System;

namespace OnceMi.Framework.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //添加额外的编码，按需
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //ConfigManager
            services.AddConfig();

            #region IdGenerator

            services.AddIdGenerator(x =>
            {
                x.AppId = Configuration.GetValue<ushort>("AppSettings:AppId");
            });

            #endregion

            //MemoryCache
            services.AddMemoryCache();
            //Db
            services.AddDatabase();
            //AutoMapper
            services.AddMapper();
            //RedisCahe
            services.AddRedisCache();
            //Swagger
            services.AddSwagger();
            //OSS，需要在AddRedisCache后面，OSS组件依赖于Redis
            services.AddOSS();

            #region Aop

            services.AddAop();

            #endregion

            #region Service & Repository

            services.AddRepository();
            services.AddService();

            #endregion

            #region 跨域

            services.AddCors(options =>
            {
                options.AddPolicy(GlobalConfigConstant.DefaultOriginsName, policy =>
                 {
                     policy.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowAnyOrigin();
                 });
            });

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

            #region 认证与授权

            var tokenConfig = Configuration.GetSection("TokenManagement").Get<TokenManagementNode>();
            var identityServerConfig = Configuration.GetSection("IdentityServer").Get<IdentityServerNode>();

            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(x =>
                {
                    if (identityServerConfig.IsEnabledIdentityServer)
                    {
                        #region IdentityServer

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

                        #endregion
                    }
                    else
                    {
                        #region 本地认证

                        x.RequireHttpsMetadata = false;
                        x.SaveToken = true;
                        x.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenConfig.Secret)),
                            ValidIssuer = tokenConfig.Issuer,
                            ValidAudience = tokenConfig.Audience,
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            RoleClaimType = JwtClaimTypes.Role,
                            NameClaimType = JwtClaimTypes.Name,
                            RequireExpirationTime = true, //过期时间
                            ClockSkew = TimeSpan.FromMinutes(5),
                        };

                        #endregion
                    }
                    x.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            await CustumJwtBearerEvents.OnChallenge(context);
                        },
                        OnForbidden = async context =>
                        {
                            await CustumJwtBearerEvents.OnForbidden(context);
                        },
                        OnAuthenticationFailed = async context =>
                        {
                            await CustumJwtBearerEvents.OnAuthenticationFailed(context);
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

            #region HealthCheck

            services.AddHealthCheckService();

            #endregion

            #region 请求日志

            if (Configuration.GetValue<bool>("AppSettings:IsEnabledRequestLog"))
            {
                services.AddHttpLogging(logging =>
                {
                    // Customize HTTP logging here.
                    logging.LoggingFields = HttpLoggingFields.All;
                    logging.RequestBodyLogLimit = 4096;
                    logging.ResponseBodyLogLimit = 4096;
                });
            }

            #endregion

            #region  自动注入

            services.AddAutoInjection();

            #endregion

            #region Controller

            services.AddHttpContextAccessor();
            //大量阻止同步 I/O 的操作可能会导致线程池资源不足，进而导致应用无响应。 仅在使用不支持异步 I/O 的库时，才启用 AllowSynchronousIO
            //services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);
            //services.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);

            services.AddHostedService<InitEventsService>();
            services.AddControllers(options =>
            {
                //全局异常
                options.Filters.Add(typeof(GlobalExceptionFilter));
                //封装返回数据格式
                options.Filters.Add(typeof(GlobalApiResponseFilter));
                //全局授权过滤器
                options.Filters.Add(typeof(GlobalPermissionFilter));
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
                    options.JsonSerializerOptions.Converters.Add(new ExceptionConverter());
                    options.JsonSerializerOptions.Converters.Add(new TypeConverter());
                    //小驼峰
                    options.JsonSerializerOptions.PropertyNamingPolicy = GlobalConfigConstant.DefaultJsonNamingPolicy;
                    //循环引用
                    //options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
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
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            #region 全局异常处理

            //此次注册的是全局异常处理，用于捕获Filter中无法捕获的异常（内部异常）
            //所以需要异常处理方法应该实现修改Filter中异常处理，当Filter中异常处理中，不会再次进入此处处理
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    await RewriteHelper.GlobalExceptionHandler(context, loggerFactory, GlobalConfigConstant.DefaultJsonNamingPolicy);
                });
            });

            #endregion

            #region 请求日志

            if (Configuration.GetValue<bool>("AppSettings:IsEnabledRequestLog"))
            {
                //请求日志
                app.UseHttpLogging();
            }

            #endregion

            //Swagger
            app.UseSwaggerWithUI();
            //初始化数据库
            app.UseDbSeed();
            //健康检查
            app.UseHealthChecks();
            //跨域
            app.UseCors(GlobalConfigConstant.DefaultOriginsName);
            //消息队列
            app.UseMessageQuene();
            //https重定向
            app.UseHttpsRedirection();
            app.UseRouting();
            //认证与授权
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI(Configuration);
                endpoints.MapControllers();
            });
        }
    }
}
