using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService.Admin;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    [DisallowConcurrentExecution]
    public class HttpExcuteJob : BaseJob
    {
        private readonly ILogger<HttpExcuteJob> _logger;

        public HttpExcuteJob(ILogger<HttpExcuteJob> logger
            , IJobsService jobsService) : base(jobsService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<object> Execute(IJobExecutionContext context, Entity.Admin.Jobs job)
        {
            //修改作业状态
            _logger.LogInformation("Running job 'HttpExcuteJob'!");

            context.Result = "this is job result";

            throw new Exception("11");
        }
    }
}
