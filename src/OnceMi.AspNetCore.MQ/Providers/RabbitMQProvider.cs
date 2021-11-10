using EasyNetQ;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ.Utils;
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
    sealed class RabbitMQProvider : IBaseProvider
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
            if (obj == null)
                return;

            string channel = MqHelper.CreateQueneNmae<T>(_options.AppId);
            string data = JsonUtil.SerializeToString(obj);

            await _ibus.PubSub.PublishAsync(data, channel);
        }

        public override Task Publish<T>(T obj, TimeSpan ts) where T : class
        {
            if (obj == null)
                return Task.CompletedTask;

            string channel = MqHelper.CreateQueneNmae<T>(_options.AppId);
            string data = JsonUtil.SerializeToString(obj);

            _ibus.Scheduler.FuturePublish(data, TimeSpan.FromSeconds(ts.TotalSeconds), channel);

            return Task.CompletedTask;
        }

        public override async Task<IDisposable> Subscribe<T>(string subscriptionId, Action<T> onMessage, CancellationToken cancellationToken = default) where T : class
        {
            var result = await _ibus.PubSub.SubscribeAsync<string>(subscriptionId, (data) =>
            {
                if (string.IsNullOrEmpty(data))
                {
                    onMessage(null);
                }
                T obj = JsonUtil.DeserializeStringToObject<T>(data);
                onMessage(obj);
            }
            , x =>
            {
                x.WithTopic(subscriptionId);
            }
            , cancellationToken);
            return result;
        }

        public override void Dispose()
        {
            _ibus.Dispose();
            base.Dispose();
        }
    }
}
