﻿using FreeRedis;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ.Providers.RediskDelayTask;
using OnceMi.AspNetCore.MQ.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    /// <summary>
    /// Redis消息队列
    /// </summary>
    sealed class RedisProvider : IBaseProvider
    {
        private readonly RedisClient _client;
        private readonly RedisDelayTaskService _delayTaskService;

        public RedisProvider(MqOptions options
            , ILoggerFactory logger
            , RedisClient client) : base(options, logger)
        {
            this._client = client;
            this._delayTaskService = new RedisDelayTaskService(logger, client);
            if (!_delayTaskService.Status)
            {
                _delayTaskService.Start(_client.Publish);
            }
        }

        public override Task Publish<T>(T obj) where T : class
        {
            if (obj == null)
                return Task.CompletedTask;

            string channel = MqHelper.CreateQueneNmae<T>(_options.AppId);
            string data = JsonUtil.SerializeToString(obj);

            _client.Publish(channel, data);
            return Task.CompletedTask;
        }

        public override Task Publish<T>(T obj, TimeSpan ts) where T : class
        {
            if (obj == null)
                return Task.CompletedTask;
            if (ts == TimeSpan.Zero || ts.TotalMilliseconds == 0)
            {
                return Publish(obj);
            }

            string channel = MqHelper.CreateQueneNmae<T>(_options.AppId);
            string data = JsonUtil.SerializeToString(obj);

            _delayTaskService.JobAdd(channel, data, ts);
            return Task.CompletedTask;
        }

        public override Task<IDisposable> Subscribe<T>(string subscriptionId, Action<T> onMessage, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(subscriptionId))
                return null;
            var sub = _client.Subscribe(subscriptionId, (channel, data) =>
            {
                if (data == null) return;
                if (data.GetType() == typeof(string))
                {
                    T obj = JsonUtil.DeserializeStringToObject<T>(data.ToString());
                    onMessage(obj);
                }
                else if (data is T)
                {
                    onMessage(data as T);
                }
                else
                {
                    _logger.LogWarning($"Unknow sub message type, message type is {data.GetType()}");
                }
            });
            return Task.FromResult(sub);
        }

        public override void Dispose()
        {
            if (!_options.UseExternalRedisClient)
            {
                _client.Dispose();
            }
            base.Dispose();
        }
    }
}
