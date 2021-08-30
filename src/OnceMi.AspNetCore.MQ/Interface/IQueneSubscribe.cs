using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ.Utils;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ
{
    public abstract class IQueneSubscribe<T> : ISubscribe, IDisposable where T : class
    {
        public string SubId { get; set; }

        public IMessageQueneService MqService { get; private set; }

        private bool isDisposed = false;
        private readonly ILogger _logger;
        private Task _task;
        private IDisposable _subDisposable;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public IQueneSubscribe(IMessageQueneService mqService, ILogger logger)
        {
            MqService = mqService ?? throw new ArgumentNullException(nameof(IMessageQueneService));
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger));

            this.SubId = MqHelper.CreateQueneNmae<T>(MqService.Options.AppId);
        }

        ~IQueneSubscribe()
        {
            Dispose(false);
        }

        public Task Excute()
        {
            _task = new Task(async () =>
            {
                try
                {
                    _subDisposable = await MqService.Subscribe<T>(this.SubId, p =>
                     {
                         Subscribe(p, _tokenSource.Token);
                     }, _tokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Subscribe object {typeof(T).Name} failed, {ex.Message}", ex);
                }
            }, _tokenSource.Token);
            _task.Start();
            return Task.CompletedTask;
        }

        public abstract Task Subscribe(T model, CancellationToken cancellationToken = default);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                _subDisposable?.Dispose();
                _tokenSource?.Cancel();

                int timeout = 3000;
                Stopwatch sw = Stopwatch.StartNew();
                while (_task != null && _task.Status == TaskStatus.Running)
                {
                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        break;
                    }
                    Thread.Sleep(1);
                }
                sw.Stop();

                _tokenSource?.Dispose();
                _task?.Dispose();

                if (disposing)
                {
                    //GC自动释放
                }

                isDisposed = true;
            }
        }
    }
}
