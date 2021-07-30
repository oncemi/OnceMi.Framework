using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OnceMi.Framework.Extension.Job;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Json;
using Quartz;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 作业管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly ILogger<JobController> _logger;
        private readonly IJobsService _service;
        private readonly IJobSchedulerService _jobSchedulerService;
        private readonly IMapper _mapper;

        public JobController(ILogger<JobController> logger
            , IJobsService service
            , IMapper mapper
            , IJobSchedulerService jobSchedulerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _jobSchedulerService = jobSchedulerService ?? throw new ArgumentNullException(nameof(jobSchedulerService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 创建任务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task Post(CreateJobRequest request)
        {
            if (!string.IsNullOrEmpty(request.RequestHeader))
            {
                var (isJson, json) = JsonUtil.IsJson(request.RequestHeader);
                if (isJson)
                {
                    throw new BusException(-1, "Header必须是合法的Json字符串");
                }
                request.RequestHeader = json;
            }
            if (!string.IsNullOrEmpty(request.RequestParam))
            {
                var (isJson, json) = JsonUtil.IsJson(request.RequestParam);
                if (isJson)
                {
                    throw new BusException(-1, "请求参数必须是合法的Json字符串");
                }
                request.RequestParam = json;
            }
            if (!TryValidCron(request.Cron, out string cronOut))
            {
                throw new BusException(-1, cronOut);
            }
            request.Cron = cronOut;
            //验证请求方式
            if (!Enum.TryParse(request.RequestMethod, true, out OperationType method))
            {
                throw new BusException(-1, $"不允许的操作类型：{request.RequestMethod}。");
            }
            request.RequestMethod = method.ToString();
            var job = await _service.Insert(request);

            await _jobSchedulerService.Add(job);
        }

        [HttpPost]
        [Route(nameof(Start))]
        public Task Start()
        {

            return Task.CompletedTask;
        }

        private bool TryValidCron(string source, out string result)
        {
            result = null;
            source = source.Trim();
            if (string.IsNullOrEmpty(source))
            {
                result = "Cron表达式不能为空";
                return false;
            }
            if (!source.EndsWith('?'))
            {
                source += " ?";
            }
            if (!CronExpression.IsValidExpression(source))
            {
                result = "错误的Cron表达式";
                return false;
            }
            var cron = new CronExpression(source);
            if (new CronExpression("* * * * * ?").Equals(cron))
            {
                result = "任务调用频次过于频繁，任务运行间隔至少10s";
                return false;
            }
            string cronOfmin = source.Substring(0, source.IndexOf((char)32));
            string[] cronMinArgs = cronOfmin.Split("/");
            if (cronMinArgs.Length == 2 && int.TryParse(cronMinArgs[1], out int sec) && sec < 1)
            {
                result = "任务调用频次过于频繁，任务运行间隔至少10s";
                return false;
            }
            result = source;
            return true;
        }
    }
}
