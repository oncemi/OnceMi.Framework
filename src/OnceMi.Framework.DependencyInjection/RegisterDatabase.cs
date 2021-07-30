﻿using FreeSql;
using FreeSql.Aop;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Config;
using OnceMi.Framework.DependencyInjection.Extensions;
using OnceMi.Framework.Entity;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IRepository.Base;
using OnceMi.Framework.Util.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OnceMi.Framework.DependencyInjection
{
    public static class RegisterDatabase
    {
        static readonly IdleBus<IFreeSql> ib = new IdleBus<IFreeSql>(TimeSpan.FromMinutes(10));

        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            using (var provider = services.BuildServiceProvider())
            {
                ILogger<IFreeSql> logger = provider.GetRequiredService<ILoggerFactory>()?.CreateLogger<IFreeSql>();
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                IIdGeneratorService idGenerator = provider.GetRequiredService<IIdGeneratorService>();
                IWebHostEnvironment env = provider.GetRequiredService<IWebHostEnvironment>();
                //获取所有的连接字符串
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
                var dbNameDic = connectionStrings.GroupBy(p => p.Name).ToDictionary(g => g.Key, g => g.Count());
                foreach (var item in dbNameDic)
                {
                    if (item.Value > 1)
                    {
                        throw new Exception($"Database name cannot be duplicate, there have {item.Value} db for name {item.Key}");
                    }
                }
                //获取是否为调试模式
                foreach (var item in connectionStrings)
                {
                    var fsql = new FreeSqlBuilder()
                        .UseConnectionString(item.DbType, item.ConnectionString)
                        .UseAutoSyncStructure(env.IsDevelopment())    //自动迁移
                        .CreateDatabaseIfNotExists()   //如果数据库不存在，那么自动创建数据库
                        .Build();
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
                        &&  e.Column.CsType == typeof(long) 
                        && e.Value?.ToString().Equals("0") == true
                        && (e.Property.GetCustomAttribute<KeyAttribute>(false) != null
                        || (e.Property.GetCustomAttribute<ColumnAttribute>(false) != null  && e.Property.GetCustomAttribute<ColumnAttribute>(false).IsPrimary)))
                        {
                            //生成雪花Id
                            e.Value = idGenerator.NewId();
                        }
                    };
                    ib.Register(item.Name, () => fsql);
                }
                //同步数据库
                List<IFreeSql> freeSqls = ib.GetAll();
                foreach (var item in freeSqls)
                {
                    SyncStructure(item);
                }
                //注入
                services.AddScoped<BaseUnitOfWorkManager>();
                //注入IdleBus<IFreeSql>
                services.TryAddSingleton<IdleBus<IFreeSql>>(ib);
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
            IdleBus<IFreeSql> ib = app.ApplicationServices.GetService<IdleBus<IFreeSql>>();
            ILoggerFactory logger = app.ApplicationServices.GetService<ILoggerFactory>();
            if (ib == null)
            {
                throw new Exception("Get idlebus service failed.");
            }
            foreach (var item in ib.GetAll())
            {
                InitializeDatabase seed = new InitializeDatabase(item, logger);
                seed.Begin().GetAwaiter().GetResult();
            }
            return app;
        }

        private static void SyncStructure(IFreeSql fsql)
        {
            if (!fsql.CodeFirst.IsAutoSyncStructure)
            {
                return;
            }
            List<Type> tableAssembies = new List<Type>();
            var entities = new AssemblyHelper().GetExportedTypesByInterface(typeof(IEntity));
            foreach (Type type in entities)
            {
                foreach (Attribute attribute in type.GetCustomAttributes())
                {
                    if (attribute is TableAttribute tableAttribute)
                    {
                        Type baseType = type.BaseType;
                        if (baseType != null && baseType == typeof(IBaseEntity)
                            || baseType == typeof(IBaseEntity<long>)
                            || baseType == typeof(IBaseEntity<int>)
                            || baseType == typeof(IBaseEntity<short>)
                            || baseType == typeof(IBaseEntity<byte>)
                            || baseType == typeof(IEntity))
                        {
                            tableAssembies.Add(type);
                            break;
                        }
                    }
                }
            }
            if (tableAssembies.Count == 0)
            {
                return;
            }
            fsql.CodeFirst.SyncStructure(tableAssembies.ToArray());
        }
    }
}