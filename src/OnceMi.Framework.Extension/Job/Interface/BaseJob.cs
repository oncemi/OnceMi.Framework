using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using Quartz;
using System;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public abstract class BaseJob : IJob
    {
        private readonly IJobsService _jobsService;

        public BaseJob(IJobsService jobsService)
        {
            _jobsService = jobsService ?? throw new ArgumentNullException(nameof(jobsService));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobExcuteResult result = null;
            try
            {
                if (string.IsNullOrEmpty(context.JobDetail.Key.Name) || !long.TryParse(context.JobDetail.Key.Name, out long jobId))
                {
                    throw new Exception("Get job id from job key failed.");
                }
                Entity.Admin.Jobs job = await _jobsService.QueryJobById(jobId);
                if (job == null)
                {
                    throw new Exception("Get job id from job key failed.");
                }
                job.Status = JobStatus.Running;
                job.FireCount = context.RefireCount;
                //update
                await _jobsService.Update(job);
                var obj = await Execute(context, job);

                result = new JobExcuteResult()
                {
                    IsSuccessful = true,
                    FiredTime = context.FireTimeUtc.ToLocalTime().DateTime,
                    Elapsed = context.JobRunTime.TotalMilliseconds,
                    Result = obj
                };
                context.Result = result;
            }
            catch (Exception ex)
            {
                result = new JobExcuteResult()
                {
                    IsSuccessful = false,
                    FiredTime = context.FireTimeUtc.ToLocalTime().DateTime,
                    Exception = ex,
                };
                context.Result = result;
            }
        }

        public abstract Task<object> Execute(IJobExecutionContext context, Entity.Admin.Jobs job);
    }
}
