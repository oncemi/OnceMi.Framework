using EasyNetQ;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    /// <summary>
    /// RabbitMq消息队列
    /// </summary>
    class RabbitMQProvider : IBaseProvider
    {
        private readonly IBus _ibus = null;

        public RabbitMQProvider(MqOptions options
            , ILoggerFactory logger) : base(options, logger)
        {
            //获取所有的连接字符串
            if (string.IsNullOrEmpty(_options.Connectstring))
            {
                throw new Exception("Can not get connect strings from message quene setting.");
            }
            _ibus = RabbitHutch.CreateBus(_options.Connectstring);
        }

        public override async Task Publish<T>(T obj) where T : class
        {
            await _ibus.PubSub.PublishAsync(obj);
        }

        public override async Task<IDisposable> Subscribe<T>(string subscriptionId, Action<T> onMessage, CancellationToken cancellationToken = default) where T : class
        {
            var result = await _ibus.PubSub.SubscribeAsync<T>(subscriptionId, onMessage, cancellationToken);
            if(result != null)
            {
                return result.ConsumerCancellation;
            }
            return null;
        }

        public override void Dispose()
        {
            _ibus.Dispose();
            base.Dispose();
        }
    }
}
