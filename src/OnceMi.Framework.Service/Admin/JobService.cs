using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Config;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Reflection;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class JobService : BaseService<Jobs, long>, IJobService
    {
        private readonly IJobRepository _repository;
        private readonly ILogger<JobService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly RedisClient _redis;
        private readonly ConfigManager _config;

        public JobService(IJobRepository repository
            , ILogger<JobService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper
            , RedisClient redis
            , ConfigManager config) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _accessor = accessor;
        }

        #region Query

        public async Task<Jobs> QueryJobById(long id)
        {
            Jobs job = _redis.Get<Jobs>(CacheConstant.GetJobKey(id));
            if (job == null)
            {
                job = await _repository.Where(p => p.Id == id && !p.IsDeleted && p.AppId == _config.AppSettings.AppId)
                    .Include(p => p.Group)
                    .Include(p => p.NoticeRole)
                    .NoTracking()
                    .FirstAsync();
            }
            else
            {
                return job;
            }
            if (job == null)
            {
                return null;
            }
            //只有等待运行和运行中的job才添加缓存
            if (job.Status == JobStatus.Waiting || job.Status == JobStatus.Running)
            {
                _redis.Set(CacheConstant.GetJobKey(job.Id), job);
            }
            //深拷贝
            Jobs newJob = TransExp<Jobs, Jobs>.Copy(job);
            return newJob;
        }

        public async Task<IPageResponse<JobItemResponse>> Query(IJobPageRequest request)
        {
            IPageResponse<JobItemResponse> response = new IPageResponse<JobItemResponse>();
            //查询出全部job，然后与缓存中的合并
            List<Jobs> allJobs = await _repository.Select
                .Include(p => p.Group)
                .Include(p => p.NoticeRole)
                .Where(p => !p.IsDeleted && p.AppId == _config.AppSettings.AppId).ToListAsync();
            if (allJobs.Count == 0)
            {
                return response;
            }
            //查询全部缓存
            string[] keys = _redis.Keys(Regex.Replace(CacheConstant.SystemJobKey, @"\{.*\}*", "*"));
            if (keys != null && keys.Length > 0)
            {
                foreach (var item in keys)
                {
                    Jobs cacheJob = _redis.Get<Jobs>(item);
                    if (cacheJob == null)
                    {
                        continue;
                    }
                    var index = allJobs.FindIndex(p => p.Id == cacheJob.Id);
                    if (index < 0)
                    {
                        continue;
                    }
                    allJobs.RemoveAt(index);
                    allJobs.Add(cacheJob);
                }
            }
            //默认排序
            if(request.OrderByModels == null || request.OrderByModels.Count == 0)
            {
                request.OrderBy = new string[] { $"{nameof(Jobs.Id)},desc" };
            }
            //get order result
            var selector = allJobs
                .Where(p => !p.IsDeleted)
                .OrderBy(request.OrderByModels);
            if (!string.IsNullOrEmpty(request.Search))
            {
                selector = selector.Where(p => p.Name.Contains(request.Search));
            }
            if (!string.IsNullOrEmpty(request.RequestMethod))
            {
                selector = selector.Where(p => p.RequestMethod.Equals(request.RequestMethod, StringComparison.OrdinalIgnoreCase));
            }
            if (request.Status != null && request.Status > 0)
            {
                selector = selector.Where(p => p.Status == request.Status);
            }
            if (request.CreateTime != null)
            {
                selector = selector.Where(p => p.CreatedTime > request.CreateTime);
            }
            //get count
            int count = selector.Count();
            //get page
            List<Jobs> pageData = selector
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToList();
            return new IPageResponse<JobItemResponse>()
            {
                Page = request.Page,
                Size = pageData.Count,
                Count = count,
                PageData = _mapper.Map<List<JobItemResponse>>(pageData),
            };
        }

        public async Task<List<Jobs>> QueryInitJobs()
        {
            var allJobs = await _repository
                .Where(p => !p.IsDeleted
                    && p.IsEnabled
                    && (p.EndTime == null || (p.EndTime != null && p.EndTime > DateTime.Now))
                    && (p.Status == JobStatus.Running || p.Status == JobStatus.Waiting))
                .NoTracking()
                .ToListAsync();
            if (allJobs == null)
            {
                return new List<Jobs>();
            }
            return allJobs;
        }

        #endregion

        #region Insert

        public async Task<Jobs> Insert(CreateJobRequest request)
        {
            Jobs job = _mapper.Map<Jobs>(request);
            if (job == null)
            {
                throw new Exception($"Map '{nameof(CreateOrganizeRequest)}' DTO to '{nameof(Jobs)}' entity failed.");
            }
            if (!await _repository.Orm.Select<JobGroups>().AnyAsync(p => p.Id == request.GroupId && !p.IsDeleted))
            {
                throw new BusException(ResultCodeConstant.JOB_GROUP_NOT_EXISTS, "所选分组不存在");
            }
            job.Id = _idGenerator.NewId();
            job.FireCount = 0;
            job.Status = request.IsEnabled ? JobStatus.Stopped : JobStatus.Paused;
            job.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            job.CreatedTime = DateTime.Now;
            job.AppId = _config.AppSettings.AppId;
            //save
            await _repository.InsertAsync(job);
            var result = await QueryJobById(job.Id);
            if (result == null)
                throw new BusException(ResultCodeConstant.JOB_DATA_SAVE_ERROR, "保存任务信息到数据库失败");
            return result;
        }

        #endregion

        #region Update

        public async Task Update(UpdateJobRequest request)
        {
            Jobs job = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (job == null)
            {
                throw new BusException(ResultCodeConstant.JOB_UPDATE_ITEM_NOT_EXISTS, "修改的条目不存在");
            }
            if (!await _repository.Orm.Select<JobGroups>().AnyAsync(p => p.Id == request.GroupId && !p.IsDeleted))
            {
                throw new BusException(ResultCodeConstant.JOB_GROUP_NOT_EXISTS, "所选分组不存在");
            }
            //set value
            job = request.MapTo(job);
            job.UpdatedTime = DateTime.Now;
            job.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            await _repository.UpdateAsync(job);
            //移除缓存
            _redis.Del(CacheConstant.GetJobKey(job.Id));
        }

        public async Task UpdateJobStatus(long jobId, JobStatus status, bool isSaveToDb = false)
        {
            await UpdateJobStatus(jobId, status, null, null, null, isSaveToDb);
        }

        public async Task UpdateJobStatus(long jobId
            , JobStatus status
            , int? fireCount
            , DateTime? fireTime
            , DateTime? nextFireTime
            , bool isSaveToDb = false)
        {
            var job = await QueryJobById(jobId);
            if (job == null)
                throw new Exception($"Can not get job by id '{jobId}'");
            job.Status = status;
            job.FireCount = fireCount == null || fireCount == 0 ? job.FireCount : job.FireCount + 1;
            job.LastFireTime = fireTime == null ? job.LastFireTime : fireTime.Value;
            if(nextFireTime == null && status == JobStatus.Stopped)
                job.NextFireTime = null;
            else
                job.NextFireTime = nextFireTime == null ? job.NextFireTime : nextFireTime.Value;

            //update redis
            if (job.Status == JobStatus.Running || job.Status == JobStatus.Waiting)
            {
                _redis.Set(CacheConstant.GetJobKey(job.Id), job);
            }
            else
            {
                //作业停止或暂停后，从缓存中移除
                _redis.Del(CacheConstant.GetJobKey(job.Id));
                //强制更新至数据库
                isSaveToDb = true;
            }
            //save to db
            if (isSaveToDb)
            {
                int result = await _repository.Orm.Update<Jobs>()
                    .Set(p => p.Status, job.Status)
                    .Set(p => p.FireCount, job.FireCount)
                    .Set(p => p.LastFireTime, job.LastFireTime)
                    .Set(p => p.NextFireTime, job.NextFireTime)
                    .Where(p => p.Id == job.Id)
                    .ExecuteAffrowsAsync();
                if (result <= 0)
                {
                    _logger.LogWarning($"Update job stauts failed, when finished job. Job id is {job.Id}");
                }
            }
        }

        public async Task DisableJob(long jobId, bool isDisable)
        {
            var job = await QueryJobById(jobId);
            if (job == null)
                throw new Exception($"Can not get job by id '{jobId}'");
            JobStatus status;
            if ((job.StartTime == null || (job.StartTime != null && job.StartTime <= DateTime.Now))
                && (job.EndTime == null || (job.EndTime != null && job.EndTime > DateTime.Now))
                && !isDisable)
            {
                status = JobStatus.Waiting;
            }
            else
            {
                status = JobStatus.Stopped;
                //作业停止或暂停后，从缓存中移除
                _redis.Del(CacheConstant.GetJobKey(job.Id));
            }
            await _repository.Where(p => p.Id == jobId)
                .ToUpdate()
                .Set(p => p.IsEnabled, !isDisable)
                .Set(p => p.Status, status)
                .ExecuteAffrowsAsync();
        }

        public async Task UpdateEndTimeJob()
        {
            await _repository.Where(p => (p.EndTime <= DateTime.Now || !p.IsEnabled) && p.Status != JobStatus.Stopped && !p.IsDeleted)
                .ToUpdate()
                .Set(p => p.Status, JobStatus.Stopped)
                .Set(p => p.UpdatedTime, DateTime.Now)
                .ExecuteAffrowsAsync();
        }

        #endregion

        #region Delete

        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(ResultCodeConstant.JOB_DELETE_ITEM_NOT_EXISTS, "没有要删除的条目");
            }
            foreach (var item in ids)
            {
                var job = await QueryJobById(item);
                if (job != null && job.Status != JobStatus.Stopped)
                {
                    throw new BusException(ResultCodeConstant.JOB_IS_RUNNING, $"任务[{job.Name}]未停止，请先停止任务");
                }
            }
            await _repository.Where(p => ids.Contains(p.Id))
                .ToUpdate()
                .Set(p => p.IsDeleted, true)
                .Set(p => p.UpdatedUserId, _accessor?.HttpContext?.User?.GetSubject().id)
                .ExecuteAffrowsAsync();
        }

        #endregion

    }
}
