using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.IService.Admin;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public class JobListener : IJobListener
    {
        private readonly ILogger _logger;
        private readonly IJobsService _jobsService;
        private readonly IMessageQueneService _messageQuene;

        public JobListener(ILogger logger
            , IJobsService jobsService
            , IMessageQueneService messageQuene)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobsService = jobsService ?? throw new ArgumentNullException(nameof(jobsService));
            _messageQuene = messageQuene ?? throw new ArgumentNullException(nameof(messageQuene));
        }

        public string Name { get; } = "1110";

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"作业{context.JobDetail.Key.Name}被拒绝。");
            return Task.CompletedTask;
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"作业{context.JobDetail.Description}开始执行。");
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            //写入执行结束日志，执行时间，结果，执行状态

            //修改作业状态，作业状态，下次执行时间

            _logger.LogInformation($"作业{context.JobDetail.Description}执行完成，执行结果:{context.Result}。");
            return Task.CompletedTask;
        }
    }
}
