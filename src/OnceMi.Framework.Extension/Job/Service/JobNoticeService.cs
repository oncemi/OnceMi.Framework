using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
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

        public JobNoticeService(ILogger<JobNoticeService> logger
            , IJobService jobsService
            , IRoleService rolesService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobsService = jobsService ?? throw new ArgumentNullException(nameof(jobsService));
            _rolesService = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
        }

        public async Task Send(long jobId, JobExcuteResult result)
        {
            try
            {
                Jobs job = result.Job;
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
                //发送通知，这里还没写
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送作业执行通知失败，错误：{ex.Message}");
            }
        }
    }
}
