using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Subscribers
{
    /// <summary>
    /// 写作业历史记录
    /// </summary>
    public class JobHistorySubscribeService : IQueneSubscribe<JobHistory>
    {
        private readonly ILogger<JobHistorySubscribeService> _logger;
        private IJobHistoryService _jobHistoriesService;
        private readonly IServiceProvider _serviceProvider;
        private static readonly object locker = new object();

        public JobHistorySubscribeService(ILogger<JobHistorySubscribeService> logger
            , IMessageQueneService bus
            , IServiceProvider serviceProvider) : base(bus, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<JobHistorySubscribeService>));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public override async Task Subscribe(JobHistory model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null) return;
                if (_jobHistoriesService == null)
                {
                    lock (locker)
                    {
                        if (_jobHistoriesService == null)
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                _jobHistoriesService = scope.ServiceProvider.GetRequiredService<IJobHistoryService>();
                            }
                        }
                    }
                }
                await _jobHistoriesService.InsertAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Write job history failed, {ex.Message}", ex);
            }
        }
    }
}
