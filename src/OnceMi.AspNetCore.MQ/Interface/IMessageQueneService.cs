using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    public interface IMessageQueneService : IDisposable
    {
        public MqOptions Options { get; set; }

        /// <summary>
        /// 发布
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Task Publish<T>(T obj) where T : class;

        /// <summary>
        /// 订阅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriptionId"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        public Task<IDisposable> Subscribe<T>(string subscriptionId, Action<T> onMessage, CancellationToken cancellationToken = default) where T : class;
    }
}
