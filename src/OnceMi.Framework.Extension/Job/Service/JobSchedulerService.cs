using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly ILogger<JobSchedulerService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IScheduler _scheduler;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobsService _jobsService;
        private readonly IMessageQueneService _messageQuene;

        public JobSchedulerService(ILoggerFactory loggerFactory
            , ISchedulerFactory schedulerFactory
            , IJobsService jobsService
            , IMessageQueneService messageQuene)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<JobSchedulerService>();
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            //create scheduler
            this._scheduler = _schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            if (this._scheduler == null)
            {
                throw new Exception("Can not get scheduler from scheduler factory.");
            }
            _jobsService = jobsService ?? throw new ArgumentNullException(nameof(jobsService));
            _messageQuene = messageQuene ?? throw new ArgumentNullException(nameof(messageQuene));
        }

        public async Task Add(JobItemResponse job)
        {
            // define the job and tie it to our HelloJob class
            IJobDetail jobDetail = CreateJob(job);
            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = CreateTrigger(job);

            _scheduler.ListenerManager.AddJobListener(new JobListener(_loggerFactory.CreateLogger(job.Name), _jobsService, _messageQuene)
                , KeyMatcher<JobKey>.KeyEquals(jobDetail.Key));
            // Tell quartz to schedule the job using our trigger
            await _scheduler.ScheduleJob(jobDetail, trigger);

            await _scheduler.PauseJob(jobDetail.Key);
            //write history
            await _messageQuene.Publish(new JobHistories()
            {
                JobId = job.Id,
                CreatedTime = DateTime.Now,
                Elapsed = 0,
                Remark = "创建任务",
                Stage = JobStage.Stopped,
            });
        }

        public async Task Init()
        {
            _logger.LogInformation("Start loading jobs...");

            var allJobs = await _jobsService
                    .Where(p => !p.IsDeleted && p.IsEnabled && (p.EndTime == null || (p.EndTime != null && p.EndTime > DateTime.Now)) && p.Status == JobStatus.Running)
                    .ToListAsync();
        }

        #region Create job and trigger

        private IJobDetail CreateJob(JobItemResponse jobItem)
        {
            return CreateJob(jobItem.Id, jobItem.GroupCode, jobItem.Name);
        }

        private IJobDetail CreateJob(long jobId, string groupCode, string name)
        {
            return JobBuilder.Create<HttpExcuteJob>()
                .WithIdentity(new JobKey(jobId.ToString(), groupCode))
                .WithDescription(name)
                .Build();
        }

        private ITrigger CreateTrigger(JobItemResponse jobItem)
        {
            return CreateTrigger(jobItem.Id, jobItem.GroupCode, jobItem.Name, jobItem.Cron, jobItem.StartTime, jobItem.EndTime);
        }

        private ITrigger CreateTrigger(long jobId, string groupCode, string name, string cron, DateTime? startTime, DateTime? endTime)
        {
            if (endTime != null && endTime < DateTime.Now)
            {
                throw new BusException(-1, "任务终止时间不能小于当前时间");
            }
            if (endTime != null && endTime != null && endTime.Value < startTime.Value)
            {
                throw new BusException(-1, "任务终止时间不能小于起始时间");
            }

            DateTime nowTime = DateTime.Now;

            var triggerBuilder = TriggerBuilder
                .Create()
                .WithIdentity($"{jobId}_trigger", groupCode)
                .WithDescription(name)
                .WithCronSchedule(cron);
            if (startTime != null)
            {
                if (startTime.Value < nowTime)
                    startTime = nowTime;
                if (startTime < nowTime.AddSeconds(2))
                    startTime = startTime.Value.AddSeconds(2);
                triggerBuilder.StartAt(startTime.Value);
            }
            if (endTime != null)
            {
                triggerBuilder.EndAt(endTime.Value);
            }
            return triggerBuilder.Build();
        }

        #endregion
    }
}
