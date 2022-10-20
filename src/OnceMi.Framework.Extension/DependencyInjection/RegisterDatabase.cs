using FreeSql;
using FreeSql.Aop;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.AutoInjection;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Config;
using OnceMi.Framework.Entity;
using OnceMi.Framework.Extension.Database;
using OnceMi.Framework.IRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OnceMi.Framework.Extension.DependencyInjection
{
    public static class RegisterDatabase
    {
        static readonly IdleBus<IFreeSql> ib = new IdleBus<IFreeSql>(TimeSpan.FromMinutes(60));

        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            using (var provider = services.BuildServiceProvider())
            {
                ILogger<IFreeSql> logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<IFreeSql>();
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                IIdGeneratorService idGenerator = provider.GetRequiredService<IIdGeneratorService>();
                IWebHostEnvironment env = provider.GetRequiredService<IWebHostEnvironment>();
                //获取所有的连接字符串
                List<DbConnectionStringsNode> connectionStrings = GetConnectionStrings(configuration);
                //创建IFreeSql对象
                foreach (var item in connectionStrings)
                {
                    var registerResult = ib.TryRegister(item.Name, () =>
                    {
                        //是否开启自动迁移
                        bool syncStructure = IsAutoSyncStructure(item, env);
                        //create builder
                        FreeSqlBuilder fsqlBuilder = new FreeSqlBuilder()
                            .UseConnectionString(item.DbType, item.ConnectionString)
                            .UseAutoSyncStructure(syncStructure);
                        //如果数据库不存在，那么自动创建数据库
                        if (syncStructure && (item.DbType == FreeSql.DataType.MySql
                        || item.DbType == FreeSql.DataType.SqlServer
                        || item.DbType == FreeSql.DataType.PostgreSQL
                        || item.DbType == FreeSql.DataType.Sqlite
                        || item.DbType == FreeSql.DataType.OdbcSqlServer))
                        {
                            fsqlBuilder.CreateDatabaseIfNotExists();
                        }
                        //判断是否开启读写分离
                        if (item.Slaves != null && item.Slaves.Length > 0)
                        {
                            fsqlBuilder.UseSlave(item.Slaves);
                        }
                        IFreeSql fsql = fsqlBuilder.Build();
                        //sql执行日志
                        fsql.Aop.CurdAfter += (s, e) =>
                        {
                            logger.LogDebug($"{item.Name}(thread-{Thread.CurrentThread.ManagedThreadId}):\n  Namespace: {e.EntityType.FullName} \nElapsedTime: {e.ElapsedMilliseconds}ms \n        SQL: {e.Sql}");
                        };
                        //审计
                        fsql.Aop.AuditValue += (s, e) =>
                        {
                            //插入操作，如果是long类型的主键为0，则生成雪花Id
                            if ((e.AuditValueType == AuditValueType.Insert || e.AuditValueType == AuditValueType.InsertOrUpdate)
                             && e.Column.CsType == typeof(long)
                             && e.Value?.ToString().Equals("0") == true
                             && (e.Property.GetCustomAttribute<KeyAttribute>(false) != null || (e.Property.GetCustomAttribute<ColumnAttribute>(false)?.IsPrimary == true)))
                            {
                                //生成雪花Id
                                e.Value = idGenerator.CreateId();
                            }
                        };
                        return fsql;
                    });
                    if (!registerResult)
                    {
                        throw new Exception($"Register db '{item.Name}' failed.");
                    }
                }
                //注入
                services.AddScoped<BaseUnitOfWorkManager>();
                //注入IdleBus<IFreeSql>
                services.TryAddSingleton(ib);
                return services;
            }
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseDbSeed(this IApplicationBuilder app)
        {
            IdleBus<IFreeSql> ib = app.ApplicationServices.GetRequiredService<IdleBus<IFreeSql>>();
            if (ib == null)
            {
                throw new Exception("Get idlebus service failed.");
            }
            IConfiguration configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            IWebHostEnvironment env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            ILoggerFactory loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(nameof(RegisterDatabase));
            //获取所有的连接字符串
            List<DbConnectionStringsNode> connectionStrings = GetConnectionStrings(configuration);
            //配置文件中开启了初始化数据库，并开启了开发者模式
            foreach (var item in connectionStrings)
            {
                if (!IsAutoSyncStructure(item, env))
                {
                    continue;
                }
                IFreeSql db = ib.Get(item.Name);
                if (db == null)
                {
                    throw new Exception($"Can not get db '{item.Name}' from IdleBus.");
                }
                logger.LogInformation($"For db '{item.Name}', automatic sync database structure is turned on, start seeding database...");
                //同步表结构
                SyncStructure(db);
                //写入种子数据
                new InitializeDatabase(db, loggerFactory).Begin()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            return app;
        }

        /// <summary>
        /// 同步表结构
        /// </summary>
        /// <param name="fsql"></param>
        private static void SyncStructure(IFreeSql fsql)
        {
            if (!fsql.CodeFirst.IsAutoSyncStructure)
            {
                return;
            }
            AssemblyLoader assemblyLoader = new AssemblyLoader(p => p.Name.StartsWith(GlobalConfigConstant.FirstNamespace, StringComparison.OrdinalIgnoreCase));
            List<Type> tableAssembies = new List<Type>();
            var entities = assemblyLoader.GetExportedTypesByInterface(typeof(IEntity));
            foreach (Type type in entities)
            {
                if (type.GetCustomAttribute<TableAttribute>() != null
                    && type.GetCustomAttribute<DisableSyncStructureAttribute>() == null
                    && type.BaseType != null
                    && (type.BaseType == typeof(IBaseEntity)
                    || type.BaseType == typeof(IBaseEntity<long>)
                    || type.BaseType == typeof(IBaseEntity<int>)))
                {
                    tableAssembies.Add(type);
                }
            }
            if (tableAssembies.Count == 0)
            {
                return;
            }
            fsql.CodeFirst.SyncStructure(tableAssembies.ToArray());
        }

        /// <summary>
        /// 是否开启表结构同步
        /// </summary>
        /// <param name="dbConfig"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        private static bool IsAutoSyncStructure(DbConnectionStringsNode dbConfig, IWebHostEnvironment env)
        {
            string seedDbEnvVal = Environment.GetEnvironmentVariable("ASPNETCORE_INITDB");
            return dbConfig.AutoSyncStructure
                && (env.IsDevelopment() || bool.TryParse(seedDbEnvVal, out bool isSeedDbInEnv) && isSeedDbInEnv);
        }

        /// <summary>
        /// 从IConfiguration中获取连接字符串
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static List<DbConnectionStringsNode> GetConnectionStrings(IConfiguration configuration)
        {
            if (configuration == null)
            {
                return null;
            }
            IConfigurationSection section = configuration.GetSection("DbConnectionStrings");
            if (section == null || !section.Exists())
            {
                throw new Exception("Can not get connect strings from app setting.");
            }
            List<DbConnectionStringsNode> connectionStrings = section.Get<List<DbConnectionStringsNode>>();
            if (connectionStrings == null || connectionStrings.Count == 0)
            {
                throw new Exception("Can not get connect strings from app setting.");
            }
            //判断name是否重复
            var dbNameDic = connectionStrings.GroupBy(p => p.Name).ToDictionary(g => g.Key, g => g.Count());
            foreach (var item in dbNameDic)
            {
                if (item.Value > 1)
                {
                    throw new Exception($"Database name cannot be duplicate, there have {item.Value} db for name {item.Key}");
                }
            }
            return connectionStrings;
        }
    }
}
