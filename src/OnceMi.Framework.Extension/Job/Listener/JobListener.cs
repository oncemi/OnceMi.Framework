using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Util.Json;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public class JobListener : IJobListener
    {
        private readonly ILogger<JobListener> _logger;
        private readonly IJobService _jobsService;
        private readonly IMessageQueneService _messageQuene;
        private readonly IJobNoticeService _notice;

        public string Name { get; }

        public JobListener(ILogger<JobListener> logger
            , IJobService jobsService
            , IMessageQueneService messageQuene
            , IJobNoticeService notice
            , string jobListenerName)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobsService = jobsService ?? throw new ArgumentNullException(nameof(jobsService));
            _messageQuene = messageQuene ?? throw new ArgumentNullException(nameof(messageQuene));
            _notice = notice ?? throw new ArgumentNullException(nameof(notice));
            this.Name = string.IsNullOrEmpty(jobListenerName) ? throw new ArgumentNullException(nameof(jobListenerName)) : jobListenerName;
        }

        public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            await UpdateJobEndStatus(context, true);
        }

        public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            await UpdateJobStartStatus(context);
        }

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            await UpdateJobEndStatus(context, false);
        }

        private async Task UpdateJobStartStatus(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation($"作业【{context.JobDetail.Description}】开始执行。");

                long jobId = context.JobDetail.Key.GetId();
                //设置作业状态
                await _jobsService.UpdateJobStatus(jobId
                    , JobStatus.Running
                    , 1
                    , context.FireTimeUtc.ToLocalTime().DateTime
                    , context.NextFireTimeUtc?.ToLocalTime().DateTime
                    , false);
                //写记录
                JobHistories histories = new JobHistories()
                {
                    JobId = jobId,
                    FiredTime = context.FireTimeUtc.ToLocalTime().DateTime,
                    EndTime = DateTime.Now,
                    Elapsed = (int)context.JobRunTime.TotalMilliseconds,
                    Result = context.Result != null ? JsonUtil.SerializeToString(context.Result) : null,
                    Status = HistoryStatus.Success,
                    Stage = HistoryStage.Starting,
                    NextFiredTime = null,
                    Remark = "任务开始",
                    CreatedTime = DateTime.Now,
                };
                await _messageQuene.Publish(histories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"设置作业【{context.JobDetail.Description}】启动状态失败，错误：{ex.Message}", ex);
            }
        }

        private async Task UpdateJobEndStatus(IJobExecutionContext context, bool isVetoed)
        {
            try
            {
                string resultStr = context.Result == null ? null : JsonUtil.SerializeToString(context.Result);
                _logger.LogInformation($"作业【{context.JobDetail.Description}】{(isVetoed ? "被取消执行" : $"执行完成，结果：{resultStr}")}。");

                object endTimeObj = context.JobDetail.JobDataMap.Get(JobConstant.EndTime);
                if (endTimeObj == null || endTimeObj is not DateTime)
                {
                    throw new Exception("Can not update job status, can not get end time from job detail.");
                }
                long jobId = context.JobDetail.Key.GetId();
                //设置作业状态
                JobStatus status = JobStatus.Waiting;
                //判断是否为主动执行
                object isTriggerObj = context.MergedJobDataMap.Get(JobConstant.IsTrigger);
                if (isTriggerObj != null && (bool)isTriggerObj)
                {
                    if ((DateTime)endTimeObj <= DateTime.Now)
                    {
                        status = JobStatus.Stopped;
                    }
                }
                else
                {
                    if ((DateTime)endTimeObj <= DateTime.Now || context.NextFireTimeUtc == null)
                    {
                        status = JobStatus.Stopped;
                    }
                }
                //result
                JobExcuteResult result = (context.Result != null && context.Result is JobExcuteResult) ? (context.Result as JobExcuteResult) : null;
                if (result == null)
                {
                    throw new Exception($"获取作业【{context.JobDetail.Description}】执行结果失败");
                }
                //发送通知，需要在更新记录之前，避免记录更新异常了就无法发送通知
                await _notice.Send(jobId, result);
                //更新作业状态
                await _jobsService.UpdateJobStatus(jobId, status, true);
                //写记录
                JobHistories histories = new JobHistories()
                {
                    JobId = jobId,
                    FiredTime = context.FireTimeUtc.ToLocalTime().DateTime,
                    EndTime = DateTime.Now,
                    Elapsed = (int)context.JobRunTime.TotalMilliseconds,
                    Status = isVetoed ? HistoryStatus.Vetoed : (result.IsSuccessful ? HistoryStatus.Success : HistoryStatus.Failed),
                    Result = resultStr,
                    Stage = HistoryStage.Stopped,
                    NextFiredTime = context.NextFireTimeUtc?.ToLocalTime().DateTime,
                    Remark = isVetoed ? "任务终止" : "任务结束",
                    CreatedTime = DateTime.Now,
                };
                await _messageQuene.Publish(histories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"设置作业【{context.JobDetail.Description}】结束状态失败，错误：{ex.Message}", ex);
            }
        }
    }
}
