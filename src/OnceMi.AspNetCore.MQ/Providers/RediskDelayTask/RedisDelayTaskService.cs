using FreeRedis;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.MQ.Providers.RediskDelayTask
{
    sealed class RedisDelayTaskService : IDisposable
    {
        private readonly ILogger<RedisDelayTaskService> _logger;
        private readonly RedisClient _client;
        private readonly BucketManager _bucket;

        private const int PRE_READ_SECONDS = 60000;  //预读时间，单位：毫秒
        private const int PRE_SEND_MILLISECONDS = 100; //提前发送时间，单位：毫秒
        private bool isWaiting = false;
        private bool isCanceling = false;
        private CancellationTokenSource _cancellation = null;
        private Task _workTask = null;
        private readonly SortedList<long, JobMember> _tempList = new SortedList<long, JobMember>();

        private readonly object _cancelDelayLocker = new object();
        private static readonly object _startLocker = new object();

        public bool Status { get; private set; } = false;

        public RedisDelayTaskService(ILoggerFactory logger
            , RedisClient client)
        {
            this._logger = logger.CreateLogger<RedisDelayTaskService>();
            this._client = client ?? throw new ArgumentNullException(nameof(client));
            this._bucket = new BucketManager(_client);
        }

        public JobMember JobAdd(string channel, string data, TimeSpan delay)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            //添加到redis中
            JobMember member = _bucket.Add(channel, data, delay);
            //取消等待
            CancelDelay();
            return member;
        }

        public void JobDelete(JobMember member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }
            _bucket.Delete(member.Id);
        }

        public void JobDelete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            _bucket.Delete(id);
        }

        public void Start(Func<string, string, long> publish)
        {
            if (!this.Status)
            {
                lock (_startLocker)
                {
                    if (!this.Status)
                    {
                        _workTask = Task.Factory.StartNew(async () =>
                        {
                            await WorkTask(publish);
                        }, TaskCreationOptions.LongRunning);

                        if (_workTask.Status != TaskStatus.Running)
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            int timeout = 30000;
                            while (_workTask.Status != TaskStatus.Running)
                            {
                                Thread.Sleep(1);
                                if (sw.ElapsedMilliseconds > timeout)
                                {
                                    throw new Exception("Redis delay task start failed.");
                                }
                            }
                        }
                        Status = true;
                    }
                }
            }
        }

        #region private

        /// <summary>
        /// 工作线程
        /// </summary>
        /// <param name="publish"></param>
        /// <returns></returns>
        private async Task WorkTask(Func<string, string, long> publish)
        {
            while (true)
            {
                //_logger.LogWarning($"Test log, main task restart, Now method {nameof(Start)} thread is is {Thread.CurrentThread.ManagedThreadId}");
                try
                {
                    JobMember job = _bucket.Min();
                    if (job == null)
                    {
                        //10分钟重新进入
                        await Delay(60000);
                        continue;
                    }
                    long interval = GetInterval(NowTicks(), job.Source);
                    if (interval > PRE_SEND_MILLISECONDS)
                    {
                        //延时大于预发送时间才有delay意义，否者直接发送
                        bool isInterrupt = await Delay((int)interval);
                        //非中断表示延时到期，需要发送
                        if (!isInterrupt && GetInterval(NowTicks(), job.Source) < PRE_SEND_MILLISECONDS)
                        {
                            publish(job.Channel, job.Data);
                            _bucket.Delete(job.Id);
                        }
                        continue;
                    }
                    else
                    {
                        publish(job.Channel, job.Data);
                        _bucket.Delete(job.Id);
                    }
                    Thread.Yield();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Redis delay task excute warning, {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Delay
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        private async Task<bool> Delay(int milliseconds)
        {
            //_logger.LogWarning($"Test log, start delay, Now method {nameof(Delay)} thread is is {Thread.CurrentThread.ManagedThreadId}");

            if (_cancellation != null)
            {
                CancelDelay();
            }
            if (_cancellation == null)
            {
                _cancellation = new CancellationTokenSource();
            }
            isWaiting = true;
            bool isInterrupt = false;
            try
            {
                await Task.Delay(milliseconds, _cancellation.Token);
                isWaiting = false;
                //延时到期后调用
                CancelDelay();
                isInterrupt = false;
            }
            catch (TaskCanceledException)
            {
                isWaiting = false;
                //give cancel operate 5 millis
                await Task.Delay(5);
                //等待取消操作完成
                if (isCanceling)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    int timeout = 30000;
                    while (isCanceling)
                    {
                        await Task.Delay(5);
                        if (sw.ElapsedMilliseconds > timeout)
                        {
                            throw new Exception("Cancel timeout, cancel failed.");
                        }
                    }
                }
                isInterrupt = true;
            }
            //_logger.LogWarning($"Test log, end delay, Now method {nameof(Delay)} thread is is {Thread.CurrentThread.ManagedThreadId}");
            return isInterrupt;
        }

        /// <summary>
        /// 取消延时
        /// </summary>
        private void CancelDelay()
        {
            try
            {
                lock (_cancelDelayLocker)
                {
                    //_logger.LogWarning($"Test log, start cancel, Now method {nameof(CancelDelay)} thread is is {Thread.CurrentThread.ManagedThreadId}");
                    if (isCanceling)
                    {
                        return;
                    }
                    if (_cancellation == null)
                    {
                        return;
                    }
                    isCanceling = true;
                    if (!_cancellation.IsCancellationRequested)
                    {
                        _cancellation.Cancel();
                    }
                    if (isWaiting)
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        int timeout = 30000;
                        while (isWaiting)
                        {
                            Thread.Sleep(1);
                            if (sw.ElapsedMilliseconds > timeout)
                            {
                                throw new Exception("Cancel timeout, cancel failed.");
                            }
                        }
                    }
                    _cancellation.Dispose();
                    _cancellation = null;
                }
            }
            finally
            {
                isCanceling = false;
                //_logger.LogWarning($"Test log, end cancel, Now method {nameof(CancelDelay)} thread is is {Thread.CurrentThread.ManagedThreadId}");
            }
        }

        /// <summary>
        /// 获取两个时间之间间隔的毫秒数
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private long GetInterval(long start, long end)
        {
            long interval = (end - start) / 10000;
            return interval;
        }

        /// <summary>
        /// 当前时间Ticks
        /// </summary>
        /// <returns></returns>
        public long NowTicks()
        {
            return DateTime.Now.ToUniversalTime().Ticks - 621355968000000000;
        }

        public void Dispose()
        {
            //结束工作线程
        }

        #endregion
    }
}
