using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    interface IProvider : IDisposable
    {
        Task Publish<T>(T obj) where T : class;

        Task Publish<T>(T obj, TimeSpan ts) where T : class;

        Task<IDisposable> Subscribe<T>(string subscriptionId, Action<T> onMessage, CancellationToken cancellationToken = default) where T : class;
    }
}
