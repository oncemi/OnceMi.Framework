using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Cache;
using OnceMi.Framework.Util.User;
using System;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class JobsService : BaseService<Jobs, long>, IJobsService
    {
        private readonly IJobsRepository _repository;
        private readonly ILogger<JobsService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly RedisClient _redis;

        public JobsService(IJobsRepository repository
            , ILogger<JobsService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper
            , RedisClient redis) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _accessor = accessor;
        }

        public async Task<Jobs> QueryJobById(long id)
        {
            var job = await _redis.GetOrCreateAsync(AdminCacheKey.GetJobKey(id), async () =>
            {
                var job = await _repository.Where(p => p.Id == id && !p.IsDeleted)
                    .Include(p => p.Group)
                    .NoTracking()
                    .FirstAsync();
                return job;
            });
            return job;
        }

        public async Task Update(Jobs job, bool isSaveToDb = false)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            //set update time
            job.UpdatedTime = DateTime.Now;
            job.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            //update redis
            _redis.Set(AdminCacheKey.GetJobKey(job.Id), job);
            //save to db
            if (isSaveToDb)
            {
                await _repository.UpdateAsync(job);
            }
        }

        public async Task<JobItemResponse> Insert(CreateJobRequest request)
        {
            Jobs job = _mapper.Map<Jobs>(request);
            if (job == null)
            {
                throw new Exception($"Map '{nameof(CreateOrganizeRequest)}' DTO to '{nameof(Jobs)}' entity failed.");
            }
            if (!await _repository.Orm.Select<JobGroups>().AnyAsync(p => p.Id == request.GroupId && !p.IsDeleted))
            {
                throw new BusException(-1, "所选分组不存在");
            }
            job.Id = _idGenerator.NewId();
            job.FireCount = 0;
            job.Status = JobStatus.Paused;
            job.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            job.CreatedTime = DateTime.Now;
            //save
            await _repository.InsertAsync(job);
            var result = await QueryJobById(job.Id);
            if (result == null)
                throw new BusException(-1, "保存任务信息到数据库失败");
            //save to redis
            _redis.Set(AdminCacheKey.GetJobKey(job.Id), job);
            return _mapper.Map<JobItemResponse>(result);
        }


        //public async Task<List<Jobs>> QueryJobsFromCache()
        //{
        //    var cacheKey =  Regex.Replace(AdminCacheKey.SystemJobKey, @"\{.*\}*", "*");
        //    var keys = _redis.Keys(cacheKey);
        //    if (keys.Length > 0)
        //    {
        //        foreach (var item in keys)
        //        {

        //        }
        //    }
        //    else
        //    {
        //        //从数据库中读
        //    }
        //}
    }
}
