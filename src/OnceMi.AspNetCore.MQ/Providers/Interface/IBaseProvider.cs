using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    abstract class IBaseProvider : IProvider
    {
        protected readonly MqOptions _options;
        protected readonly ILogger _logger;

        public IBaseProvider(MqOptions options, ILoggerFactory logger)
        {
            this._options = options;
            this._logger = logger == null ? throw new ArgumentNullException(nameof(logger)) : logger.CreateLogger(this.GetType().Name);
        }

        public virtual void Dispose()
        {
            
        }

        public abstract Task Publish<T>(T obj) where T : class;

        public abstract Task Publish<T>(T obj, TimeSpan ts) where T : class;

        public abstract Task<IDisposable> Subscribe<T>(string subscriptionId, Action<T> onMessage, CancellationToken cancellationToken = default) where T : class;
    }
}
