using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Extension.Job;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Json;
using Quartz;
using System;
using System.Collections.Generic;
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
        private readonly IJobService _service;
        private readonly IJobSchedulerService _jobSchedulerService;
        private readonly IMapper _mapper;

        public JobController(ILogger<JobController> logger
            , IJobService service
            , IMapper mapper
            , IJobSchedulerService jobSchedulerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _jobSchedulerService = jobSchedulerService ?? throw new ArgumentNullException(nameof(jobSchedulerService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IPageResponse<JobItemResponse>> Get([FromQuery] IJobPageRequest request)
        {
            return await _service.Query(request);
        }

        /// <summary>
        /// 根据ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<JobItemResponse> Get(long id)
        {
            var result = await _service.QueryJobById(id);
            if (result == null) 
                return null;
            return _mapper.Map<JobItemResponse>(result);
        }

        /// <summary>
        /// 创建任务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JobItemResponse> Post(CreateJobRequest request)
        {
            if (!string.IsNullOrEmpty(request.RequestHeader))
            {
                if (!JsonUtil.TryParse(request.RequestHeader,out string json))
                {
                    throw new BusException(ResultCodeConstant.JOB_HEADER_MUST_JSON, "Header必须是合法的Json字符串");
                }
                request.RequestHeader = json;
            }
            if (!string.IsNullOrEmpty(request.RequestParam))
            {
                if (!JsonUtil.TryParse(request.RequestParam,out string json))
                {
                    throw new BusException(ResultCodeConstant.JOB_PARAMS_MUST_JSON, "请求参数必须是合法的Json字符串");
                }
                request.RequestParam = json;
            }
            if (!TryValidCron(request.Cron, out string cronOut))
            {
                throw new BusException(ResultCodeConstant.JOB_CRON_ERROR, cronOut);
            }
            if (request.EndTime != null && request.EndTime.Value < DateTime.Now)
            {
                throw new BusException(ResultCodeConstant.JOB_END_TIME_LOWER_THAN_NOW, "任务结束时间不能小于当前时间");
            }
            if (request.StartTime != null && request.EndTime != null && request.EndTime.Value <= request.StartTime.Value)
            {
                throw new BusException(ResultCodeConstant.JOB_END_TIME_LOWER_THAN_START, "任务结束时间必须大于任务开始时间");
            }
            request.Cron = cronOut;
            //验证请求方式
            if (!Enum.TryParse(request.RequestMethod, true, out OperationType method))
            {
                throw new BusException(ResultCodeConstant.JOB_UNKNOW_OPERATION_TYPE, $"不允许的操作类型：{request.RequestMethod}");
            }
            request.RequestMethod = method.ToString();
            request.EndTime = request.EndTime ?? DateTime.MaxValue.AddSeconds(-1);
            var job = await _service.Insert(request);
            if (request.IsEnabled)
            {
                await _jobSchedulerService.Add(job);
            }
            return _mapper.Map<JobItemResponse>(job);
        }

        /// <summary>
        /// 修改任务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task Put(UpdateJobRequest request)
        {
            var job = await _service.QueryJobById(request.Id);
            if (job == null)
            {
                throw new BusException(ResultCodeConstant.JOB_NOT_EXISTS, "当前任务不存在");
            }
            if (!string.IsNullOrEmpty(request.RequestHeader))
            {
                if (!JsonUtil.TryParse(request.RequestHeader, out string json))
                {
                    throw new BusException(ResultCodeConstant.JOB_HEADER_MUST_JSON, "Header必须是合法的Json字符串");
                }
                request.RequestHeader = json;
            }
            if (!string.IsNullOrEmpty(request.RequestParam))
            {
                if (!JsonUtil.TryParse(request.RequestParam, out string json))
                {
                    throw new BusException(ResultCodeConstant.JOB_PARAMS_MUST_JSON, "请求参数必须是合法的Json字符串");
                }
                request.RequestParam = json;
            }
            if (!TryValidCron(request.Cron, out string cronOut))
            {
                throw new BusException(ResultCodeConstant.JOB_CRON_ERROR, cronOut);
            }
            if (request.EndTime != null && request.EndTime.Value < DateTime.Now)
            {
                throw new BusException(ResultCodeConstant.JOB_END_TIME_LOWER_THAN_NOW, "任务结束时间不能小于当前时间");
            }
            if (request.StartTime != null && request.EndTime != null && request.EndTime.Value <= request.StartTime.Value)
            {
                throw new BusException(ResultCodeConstant.JOB_END_TIME_LOWER_THAN_START, "任务结束时间必须大于任务开始时间");
            }
            request.Cron = cronOut;
            //验证请求方式
            if (!Enum.TryParse(request.RequestMethod, true, out OperationType method))
            {
                throw new BusException(ResultCodeConstant.JOB_UNKNOW_OPERATION_TYPE, $"不允许的操作类型：{request.RequestMethod}");
            }
            request.RequestMethod = method.ToString();
            request.EndTime = request.EndTime ?? DateTime.MaxValue.AddSeconds(-1);
            //先移除
            if (await _jobSchedulerService.Exists(job))
            {
                await _jobSchedulerService.Pause(job);
            }
            await _jobSchedulerService.Delete(job);
            //更新
            await _service.Update(request);
            if (request.IsEnabled)
            {
                job = await _service.QueryJobById(request.Id);
                if (job == null)
                {
                    throw new BusException(ResultCodeConstant.JOB_LOAD_ERROR, "更新任务数据成功，但是重新加载任务过程中出现了错误，无法获取任务数据");
                }
                await _jobSchedulerService.Add(job);
            }
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task Stop(JobOperationRequest request)
        {
            var job = await _service.QueryJobById(request.Id);
            if (job == null)
            {
                throw new BusException(ResultCodeConstant.JOB_NOT_EXISTS, "当前任务不存在");
            }
            await _jobSchedulerService.Stop(job);
            if (!job.IsEnabled)
            {
                return;
            }
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task Pause(JobOperationRequest request)
        {
            var job = await _service.QueryJobById(request.Id);
            if (job == null)
            {
                throw new BusException(ResultCodeConstant.JOB_NOT_EXISTS, "当前任务不存在");
            }
            await _jobSchedulerService.Pause(job);
        }

        /// <summary>
        /// 恢复任务
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task Resume(JobOperationRequest request)
        {
            var job = await _service.QueryJobById(request.Id);
            if (job == null)
            {
                throw new BusException(ResultCodeConstant.JOB_NOT_EXISTS, "当前任务不存在");
            }
            await _jobSchedulerService.Resume(job);
        }

        /// <summary>
        /// 立即执行
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task Trigger(JobOperationRequest request)
        {
            var job = await _service.QueryJobById(request.Id);
            if (job == null)
            {
                throw new BusException(ResultCodeConstant.JOB_NOT_EXISTS, "当前任务不存在");
            }
            if(job.Status == JobStatus.Stopped)
            {
                throw new BusException(ResultCodeConstant.JOB_STOPPED, "当前任务已停止，请先开始任务后再执行此操作");
            }
            await _jobSchedulerService.Trigger(job);
        }

        /// <summary>
        /// 根据Id删除
        /// </summary>
        [HttpDelete]
        public async Task Delete(List<long> ids)
        {
            await _service.Delete(ids);
        }

        #region private

        private bool TryValidCron(string source, out string result)
        {
            source = source.Trim();
            if (string.IsNullOrEmpty(source))
            {
                result = "Cron表达式不能为空";
                return false;
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

        #endregion

    }
}
