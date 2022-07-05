using FreeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    public class MessageQueneService : IMessageQueneService
    {
        public MqOptions Options { get; set; }

        private readonly IProvider _provider;

        public MessageQueneService(IOptionsMonitor<MqOptions> optionsMonitor
            , ILoggerFactory logger
            , IServiceProvider serviceProvider)
        {
            IOptionsMonitor<MqOptions> options = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            Options = options.Get(MqOptionSetting.Name);
            if (Options.AppId <= 0)
            {
                throw new Exception("AppId can not null.");
            }
            switch (Options.ProviderType)
            {
                case MqProviderType.RabbitMQ:
                    _provider = new RabbitMQProvider(Options, logger);
                    break;
                case MqProviderType.Redis:
                    RedisClient client;
                    if (Options.UseExternalRedisClient)
                    {
                        object redis = serviceProvider.GetService(typeof(RedisClient));
                        if (redis == null)
                            throw new Exception($"Can not get '{nameof(RedisClient)}' from DI.");
                        client = redis as RedisClient;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Options.Connectstring))
                        {
                            throw new Exception("Can not get connect strings from message quene setting.");
                        }
                        BuildRedisKeyPrefix(Options);
                        client = new RedisClient(Options.Connectstring);
                    }
                    _provider = new RedisProvider(Options, logger, client);
                    break;
            }
        }

        public async Task Publish<T>(T obj) where T : class
        {
            await _provider.Publish(obj);
        }

        public async Task Publish<T>(T obj, TimeSpan ts) where T : class
        {
            await _provider.Publish(obj, ts);
        }

        public async Task<IDisposable> Subscribe<T>(string subscriptionId, Action<T> onMessage, CancellationToken cancellationToken = default) where T : class
        {
            return await _provider.Subscribe(subscriptionId, onMessage, cancellationToken);
        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        private void BuildRedisKeyPrefix(MqOptions options)
        {
            if (options.ProviderType != MqProviderType.Redis)
            {
                return;
            }
            if (options.Connectstring.Contains("prefix", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            string prefix = $"oncemi_mq:{options.AppId}:";
            if (!string.IsNullOrWhiteSpace(options.RedisPrefix))
            {
                prefix = options.RedisPrefix;
            }
            if (options.Connectstring.EndsWith(','))
            {
                options.Connectstring += $"prefix={prefix}";
            }
            else
            {
                options.Connectstring += $",prefix={prefix}";
            }
        }
    }
}
