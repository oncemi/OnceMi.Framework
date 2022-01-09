using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly ILogger<JobSchedulerService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IScheduler _scheduler;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobService _jobsService;
        private readonly IMessageQueneService _messageQuene;
        private readonly ICacheService _cacheService;
        private readonly IJobNoticeService _jobNoticeService;

        public JobSchedulerService(ILoggerFactory loggerFactory
            , ISchedulerFactory schedulerFactory
            , IJobService jobsService
            , IMessageQueneService messageQuene
            , ICacheService cacheService
            , IJobNoticeService jobNoticeService)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<JobSchedulerService>();
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            //create scheduler
            _scheduler = _schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            if (_scheduler == null)
            {
                throw new Exception("Can not get scheduler from scheduler factory.");
            }
            _jobsService = jobsService ?? throw new ArgumentNullException(nameof(jobsService));
            _messageQuene = messageQuene ?? throw new ArgumentNullException(nameof(messageQuene));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _jobNoticeService = jobNoticeService ?? throw new ArgumentNullException(nameof(jobNoticeService));
        }

        /// <summary>
        /// 初始化作业
        /// 系统启动时，从数据库中加载之前在运行的任务
        /// 状态变更：
        /// 1、等待变更为等待
        /// 2、运行变更为等待（重新加载后需要重新开始运行）
        /// 系统启动时，先清除缓存，然后将过期任务设置为停止
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {
            _logger.LogInformation("Start loading jobs...");

            //清除job缓存
            _cacheService.DeleteCaches(new DeleteCachesRequest() { Value = CacheConstant.SystemJobKey });
            //更新到期的任务为停止
            await _jobsService.UpdateEndTimeJob();
            //重新加载任务
            var allJobs = await _jobsService.QueryInitJobs();
            foreach (var item in allJobs)
            {
                var job = await _jobsService.QueryJobById(item.Id);
                await this.Add(job, true);
            }
        }

        /// <summary>
        /// 添加作业
        /// 状态变更：
        /// 1、初始化状态为停止状态，变更为等待运行状态（立即执行）
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task Add(Jobs job)
        {
            await Add(job, false);
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task Delete(Jobs job)
        {
            var key = new JobKey(job.Id.ToString(), job.Group.Code);

            if (await _scheduler.CheckExists(key))
            {
                bool result = await _scheduler.DeleteJob(key);
                await _jobsService.UpdateJobStatus(job.Id, JobStatus.Stopped, true);
                if (!result)
                {
                    throw new Exception($"删除作业“{job.Name}”失败");
                }
            }
            else
            {
                await _jobsService.UpdateJobStatus(job.Id, JobStatus.Stopped, true);
            }
        }

        /// <summary>
        /// 暂停任务
        /// 状态变更：
        /// 从等待或运行状态变更为暂停状态，不能从停止状态变更到暂停状态
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task Pause(Jobs job)
        {
            var key = new JobKey(job.Id.ToString(), job.Group.Code);
            if (!await _scheduler.CheckExists(key))
            {
                throw new Exception($"暂停作业“{job.Name}”失败，作业不存在或未开始");
            }
            //如果作业是停止了的作业，从调度器中移除后抛出业务异常
            if (job.Status == JobStatus.Stopped)
            {
                await _scheduler.DeleteJob(key);
                throw new Exception($"暂停作业“{job.Name}”失败，作业已停止");
            }
            await _scheduler.PauseJob(key);
            //等待任务结束
            while ((await _scheduler.GetCurrentlyExecutingJobs())?.Any(p => p.JobDetail.Key.GetId() == job.Id) == true)
            {
                await Task.Delay(10);
            }
            await _jobsService.UpdateJobStatus(job.Id, JobStatus.Paused, true);
        }

        /// <summary>
        /// 恢复
        /// 状态变更：
        /// 1、从暂停状态变更为等待状态
        /// 2、从停止状态（手动停止）变更为等待状态
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task Resume(Jobs job)
        {
            var key = new JobKey(job.Id.ToString(), job.Group.Code);
            if (job.EndTime != null && job.EndTime <= DateTime.Now)
            {
                if (job.Status != JobStatus.Stopped)
                {
                    await _jobsService.UpdateJobStatus(job.Id, JobStatus.Stopped, true);
                }
                throw new BusException(ResultCode.JOB_OUT_OF_DATE, $"任务{job.Name}已过期");
            }
            //停止状态时，job被禁用，先设置job为启用状态
            if (!job.IsEnabled)
            {
                await _jobsService.DisableJob(job.Id, false);
            }
            //在恢复之前先更改状态，不然恢复后更改状态为running，然后又被更改为waiting
            await _jobsService.UpdateJobStatus(job.Id, JobStatus.Waiting, true);
            if (!await _scheduler.CheckExists(key))
            {
                await Add(job, true);
            }
            else
            {
                await _scheduler.ResumeJob(key);
            }
        }

        /// <summary>
        /// 停止作业
        /// 状态变更：
        /// 1、从等待变更为停止
        /// 2、从运行变更为停止
        /// 3、从暂停变更为停止
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task Stop(Jobs job)
        {
            var key = new JobKey(job.Id.ToString(), job.Group.Code);
            if (!await _scheduler.CheckExists(key))
            {
                if (job.Status != JobStatus.Stopped)
                {
                    await _jobsService.DisableJob(job.Id, true);
                }
            }
            else
            {
                if ((await _scheduler.GetCurrentlyExecutingJobs())?.Any(p => p.JobDetail.Key.GetId() == job.Id) == true)
                {
                    if (!await _scheduler.Interrupt(key))
                    {
                        throw new Exception($"终止正在运行的任务{job.Name}失败");
                    }
                }
                await _scheduler.PauseJob(key);
                if (!await _scheduler.DeleteJob(key))
                {
                    throw new Exception($"停止任务{job.Name}失败");
                }
                await _jobsService.DisableJob(job.Id, true);
            }
        }

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task Trigger(Jobs job)
        {
            var key = new JobKey(job.Id.ToString(), job.Group.Code);
            if (!await _scheduler.CheckExists(key))
            {
                if (job.Status != JobStatus.Paused)
                {
                    throw new Exception($"执行任务失败，任务调度处理器中，{job.Name}不存在");
                }
                await Add(job, true);
            }
            if ((await _scheduler.GetCurrentlyExecutingJobs())?.Any(p => p.JobDetail.Key.GetId() == job.Id) == true)
            {
                return;
            }
            IDictionary<string, object> map = new Dictionary<string, object>() { { "IsTrigger", true } };
            await _scheduler.TriggerJob(key, new JobDataMap(map));
        }

        public async Task<bool> Exists(Jobs job)
        {
            var key = new JobKey(job.Id.ToString(), job.Group.Code);
            return await _scheduler.CheckExists(key);
        }

        #region private

        private async Task Add(Jobs job, bool isLoading)
        {
            var key = new JobKey(job.Id.ToString(), job.Group.Code);
            if (await _scheduler.CheckExists(key))
            {
                return;
            }
            // define the job and tie it to our HelloJob class
            IJobDetail jobDetail = CreateJob(job);
            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = CreateTrigger(job);

            _scheduler.ListenerManager.AddJobListener(new JobListener(_loggerFactory.CreateLogger<JobListener>()
                , _jobsService
                , _messageQuene
                , _jobNoticeService
                , $"{job.Name}#{job.Id}")
                , KeyMatcher<JobKey>.KeyEquals(jobDetail.Key));
            // Tell quartz to schedule the job using our trigger
            await _scheduler.ScheduleJob(jobDetail, trigger);

            IReadOnlyCollection<IJobExecutionContext> runJobs = await _scheduler.GetCurrentlyExecutingJobs();
            //如果job没有运行，表示job休眠，等待运行
            if (runJobs == null || !runJobs.Any(p => p.JobDetail.Key.GetId() == job.Id))
            {
                //获取添加的任务Trigger信息
                var runJobTrigger = await _scheduler.GetTrigger(trigger.Key);
                //设置作业状态（等待运行）
                await _jobsService.UpdateJobStatus(job.Id
                    , JobStatus.Waiting
                    , null
                    , null
                    , runJobTrigger.GetNextFireTimeUtc()?.ToLocalTime().DateTime
                    , true);
            }
            //if is not load job from db, write history
            if (!isLoading)
            {
                await _messageQuene.Publish(new JobHistories()
                {
                    JobId = job.Id,
                    FiredTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    Elapsed = 0,
                    Status = HistoryStatus.Success,
                    Stage = HistoryStage.Initialized,
                    Result = null,
                    NextFiredTime = trigger.GetNextFireTimeUtc()?.ToLocalTime().DateTime,
                    Remark = "创建任务/修改任务",
                });
            }
        }

        #region Create job and trigger

        private IJobDetail CreateJob(Jobs jobItem)
        {
            IJobDetail result = JobBuilder.Create<HttpExcuteJob>()
                .WithIdentity(new JobKey(jobItem.Id.ToString(), jobItem.Group.Code))
                .WithDescription(jobItem.Name)
                .Build();
            result.JobDataMap.Put(JobConstant.Id, jobItem.Id);
            result.JobDataMap.Put(JobConstant.Name, jobItem.Name);
            result.JobDataMap.Put(JobConstant.EndTime, jobItem.EndTime);
            return result;
        }

        private ITrigger CreateTrigger(Jobs jobItem)
        {
            if (jobItem.EndTime != null && jobItem.EndTime < DateTime.Now)
            {
                throw new BusException(ResultCode.JOB_END_TIME_LOWER_THAN_NOW, "任务结束时间不能小于当前时间");
            }
            if (jobItem.StartTime != null && jobItem.EndTime != null && jobItem.EndTime.Value < jobItem.StartTime.Value)
            {
                throw new BusException(ResultCode.JOB_END_TIME_LOWER_THAN_START, "任务结束时间必须大于任务开始时间");
            }
            DateTime nowTime = DateTime.Now;
            DateTime? startTime = jobItem.StartTime;
            var triggerBuilder = TriggerBuilder
                .Create()
                .WithIdentity($"{jobItem.Id}_trigger", jobItem.Group.Code)
                .WithDescription(jobItem.Name)
                .WithCronSchedule(jobItem.Cron);
            if (startTime != null)
            {
                if (startTime.Value < nowTime)
                    startTime = nowTime;
                if (startTime < nowTime.AddSeconds(3))
                    startTime = startTime.Value.AddSeconds(3);
                triggerBuilder.StartAt(startTime.Value);
            }
            if (jobItem.EndTime != null)
            {
                triggerBuilder.EndAt(jobItem.EndTime.Value);
            }
            return triggerBuilder.Build();
        }
        #endregion

        #endregion
    }
}
