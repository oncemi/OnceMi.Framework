using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Exception;
using Quartz;
using System;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    [DisallowConcurrentExecution]
    public abstract class BaseJob : IJob
    {
        private readonly IJobsService _jobsService;
        private readonly ILogger _logger;

        public BaseJob(IJobsService jobsService
            , ILogger logger)
        {
            _jobsService = jobsService ?? throw new ArgumentNullException(nameof(jobsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobExcuteResult result = null;
            Jobs job = null;
            try
            {
                if (string.IsNullOrEmpty(context.JobDetail.Key.Name) || !long.TryParse(context.JobDetail.Key.Name, out long jobId))
                {
                    throw new Exception("Get job id from job key failed.");
                }
                job = await _jobsService.QueryJobById(jobId);
                if (job == null)
                {
                    throw new Exception("Get job id from job key failed.");
                }
                //excute
                var obj = await Execute(context, job);
                //build result
                result = new JobExcuteResult()
                {
                    IsSuccessful = true,
                    FiredTime = context.FireTimeUtc.ToLocalTime().DateTime,
                    EndTime = DateTime.Now,
                    Elapsed = context.JobRunTime.TotalMilliseconds,
                    Result = obj,
                    Job = job,
                };
                context.Result = result;
            }
            catch (JobExcuteException ex)
            {
                result = new JobExcuteResult()
                {
                    IsSuccessful = false,
                    FiredTime = context.FireTimeUtc.ToLocalTime().DateTime,
                    EndTime = DateTime.Now,
                    Elapsed = context.JobRunTime.TotalMilliseconds,
                    Result = ex.Result,
                    Exception = ex,
                    Job = job,
                };
                context.Result = result;
                _logger.LogWarning(ex, ex.Message);
            }
            catch (Exception ex)
            {
                result = new JobExcuteResult()
                {
                    IsSuccessful = false,
                    FiredTime = context.FireTimeUtc.ToLocalTime().DateTime,
                    EndTime = DateTime.Now,
                    Elapsed = context.JobRunTime.TotalMilliseconds,
                    Result = null,
                    Exception = ex,
                    Job = job,
                };
                context.Result = result;
                _logger.LogWarning(ex, ex.Message);
            }
        }

        public abstract Task<object> Execute(IJobExecutionContext context, Jobs job);
    }
}
