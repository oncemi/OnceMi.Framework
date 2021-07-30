using FreeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnceMi.Framework.Config;
using OnceMi.Framework.Util.Json;
using System;
using System.Collections.Generic;

namespace OnceMi.Framework.DependencyInjection
{
    public static class RegisterRedisCache
    {
        static RedisClient client = null;

        /// <summary>
        /// 注入缓存服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddRedisCache(this IServiceCollection services)
        {
            using (var provider = services.BuildServiceProvider())
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                IConfigurationSection section = configuration.GetSection("RedisSetting");
                if (section == null || !section.Exists())
                {
                    throw new Exception("Can not get redis connect strings from redis setting.");
                }
                RedisSettingNode redisSetting = section.Get<RedisSettingNode>();
                if (redisSetting == null
                    || redisSetting.RedisConnectionStrings == null
                    || redisSetting.RedisConnectionStrings.Count == 0)
                {
                    throw new Exception("Can not get connect strings from redis setting.");
                }
                if (redisSetting.RedisSchema == RedisSchema.Sentinel && string.IsNullOrEmpty(redisSetting.SentinelConnectionString))
                {
                    throw new Exception("When user redis sentinel, sentinel connection string can not null.");
                }
                if (redisSetting.RedisSchema == RedisSchema.MasterSlave && redisSetting.RedisConnectionStrings.Count < 2)
                {
                    throw new Exception("When user redis master-slave, must more than one redis connection string.");
                }
                switch (redisSetting.RedisSchema)
                {
                    case RedisSchema.MasterSlave:
                        {
                            ConnectionStringBuilder master = redisSetting.RedisConnectionStrings[0];
                            List<ConnectionStringBuilder> slaves = new List<ConnectionStringBuilder>();
                            for (int i = 1; i < redisSetting.RedisConnectionStrings.Count; i++)
                            {
                                slaves.Add(ConnectionStringBuilder.Parse(redisSetting.RedisConnectionStrings[i]));
                            }
                            client = new RedisClient(master, slaves.ToArray());
                        }
                        break;
                    case RedisSchema.Cluster:
                        {
                            List<ConnectionStringBuilder> clusters = new List<ConnectionStringBuilder>();
                            redisSetting.RedisConnectionStrings.ForEach(p =>
                            {
                                clusters.Add(ConnectionStringBuilder.Parse(p));
                            });
                            client = new RedisClient(clusters.ToArray());
                        }
                        break;
                    case RedisSchema.Sentinel:
                        {
                            client = new RedisClient(redisSetting.SentinelConnectionString, redisSetting.RedisConnectionStrings.ToArray(), true);
                        }
                        break;
                    case RedisSchema.Default:
                    default:
                        {
                            client = new RedisClient(redisSetting.RedisConnectionStrings[0]);
                        }
                        break;
                }
                client.Serialize = obj => JsonUtil.SerializeToString(obj);
                client.Deserialize = (json, type) => JsonUtil.DeserializeStringToObject(json, type);
                //add service
                services.TryAddSingleton<RedisClient>(client);
                return services;
            }
        }
    }
}
