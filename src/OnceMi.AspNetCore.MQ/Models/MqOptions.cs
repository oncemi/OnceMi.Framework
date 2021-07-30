using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    public class MqOptions
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string Connectstring { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public int AppId { get; set; }

        /// <summary>
        /// 消息队列提供器
        /// </summary>
        public MqProviderType ProviderType { get; set; } = MqProviderType.Redis;

        /// <summary>
        /// 使用外部的RedisClient
        /// 当在主系统注入RedisClient之后，可以将此配置设置为true。
        /// 设置true后将会从DI容器中获取已经注入的RedisClient而不再重新创建
        /// </summary>
        public bool UseExternalRedisClient { get; set; } = false;
    }
}
