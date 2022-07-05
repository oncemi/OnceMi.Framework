using FreeRedis;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Util.Date;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    public class JobNoticeService : IJobNoticeService
    {
        private readonly ILogger<JobNoticeService> _logger;
        private readonly IJobService _jobsService;
        private readonly IRoleService _rolesService;
        private readonly RedisClient _redisCache;

        //单位：秒，默认10分钟
        private const int _interval = 600;

        public JobNoticeService(ILogger<JobNoticeService> logger
            , IJobService jobsService
            , IRoleService rolesService
            , RedisClient redisCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobsService = jobsService ?? throw new ArgumentNullException(nameof(jobsService));
            _rolesService = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
            _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
        }

        public async Task Send(long jobId, JobExcuteResult result)
        {
            try
            {
                Entity.Admin.Job job = result.Job;
                if (job == null)
                    job = await _jobsService.QueryJobById(jobId);
                if (job == null)
                    throw new Exception("获取作业信息失败");

                if (job.NoticePolicy == NoticePolicy.No)
                {
                    //无需通知
                    return;
                }
                else if (job.NoticePolicy == NoticePolicy.Error && result.IsSuccessful)
                {
                    //仅异常通知
                    return;
                }
                //判断是否频繁通知
                string lastNoticeTimeStr = _redisCache.Get<string>(GlobalCacheConstant.GetJobNoticeTimeKey(job.Id));
                if (!string.IsNullOrEmpty(lastNoticeTimeStr)
                    && long.TryParse(lastNoticeTimeStr, out long lastNoticeTime))
                {
                    if (TimeUtil.Timestamp() - lastNoticeTime < 20)
                    {
                        _logger.LogInformation($"作业上次发送异常通知时间为：{lastNoticeTime}，间隔时间太短，跳过本地通知");
                        return;
                    }
                }
                //开始获取通知用户组
                if (job.NoticeRoleId == null || job.NoticeRoleId == 0)
                {
                    _logger.LogWarning($"虽然作业【{job.Name}】开启执行结果通知，但是作业未配置通知角色组");
                    return;
                }
                var users = await _rolesService.QueryRoleUsers(job.NoticeRoleId.Value);
                if (users == null || users.Count == 0)
                {
                    _logger.LogWarning($"虽然作业【{job.Name}】开启执行结果通知，但是通知角色组中未配置用户");
                }
                List<string> phones = users.Where(p => p.PhoneNumberConfirmed && !string.IsNullOrEmpty(p.PhoneNumber))
                    .Select(p => p.PhoneNumber)
                    .ToList();
                List<string> emails = users.Where(p => p.EmailConfirmed && !string.IsNullOrEmpty(p.Email))
                    .Select(p => p.Email)
                    .ToList();
                if ((phones == null || phones.Count == 0) && (emails == null || emails.Count == 0))
                {
                    _logger.LogWarning($"虽然作业【{job.Name}】开启执行结果通知，但是通知用户均未配置或未启用电话号码或邮箱");
                }
                //发送通知
                if (phones.Count > 0)
                {
                    await SendMessage(job, phones);
                }
                if (emails.Count > 0)
                {
                    await SendEmail(job, emails);
                }
                //发送完成之后写发送时间到redis
                _redisCache.Set(GlobalCacheConstant.GetJobNoticeTimeKey(jobId), TimeUtil.Timestamp().ToString(), TimeSpan.FromSeconds(_interval + new Random().Next(2, 10)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送作业执行通知失败，错误：{ex.Message}");
            }
        }

        private Task SendMessage(Entity.Admin.Job job, List<string> phones)
        {
            //发送短信的业务代码

            return Task.CompletedTask;
        }

        private Task SendEmail(Entity.Admin.Job job, List<string> emails)
        {
            //发送邮件的业务代码

            return Task.CompletedTask;
        }
    }
}
