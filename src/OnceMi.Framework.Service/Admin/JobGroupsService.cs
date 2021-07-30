using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class JobGroupsService : BaseService<JobGroups, long>, IJobGroupsService
    {
        private readonly IJobGroupsRepository _repository;
        private readonly ILogger<JobGroupsService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly RedisClient _redis;

        public JobGroupsService(IJobGroupsRepository repository
            , ILogger<JobGroupsService> logger
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

        public async Task<IPageResponse<JobGroupItemResponse>> Query(IPageRequest request)
        {
            IPageResponse<JobGroupItemResponse> response = new IPageResponse<JobGroupItemResponse>();
            Expression<Func<JobGroups, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                exp = exp.And(p => p.Name.Contains(request.Search) || p.Code.Contains(request.Search));
            }
            //get count
            long count = await _repository.Where(exp).CountAsync();
            List<JobGroups> allJobGroups = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByModels)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allJobGroups == null || allJobGroups.Count == 0)
            {
                return new IPageResponse<JobGroupItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<JobGroupItemResponse>(),
                };
            }
            return new IPageResponse<JobGroupItemResponse>()
            {
                Page = request.Page,
                Size = allJobGroups.Count,
                Count = count,
                PageData = _mapper.Map<List<JobGroupItemResponse>>(allJobGroups),
            };
        }

        public async Task<JobGroupItemResponse> Query(long id)
        {
            //查询分组
            JobGroups jobGroup = await _repository.Where(p => p.Id == id && !p.IsDeleted)
                .FirstAsync();
            if (jobGroup == null)
                return null;
            return _mapper.Map<JobGroupItemResponse>(jobGroup);
        }

        public async Task<JobGroupItemResponse> Insert(CreateJobGroupRequest request)
        {
            JobGroups jobGroup = _mapper.Map<JobGroups>(request);
            if (jobGroup == null)
            {
                throw new Exception($"Map '{nameof(CreateJobGroupRequest)}' DTO to '{nameof(JobGroups)}' entity failed.");
            }
            //判断分组是否存在
            if (await _repository.Select.AnyAsync(p => p.Name == request.Name && !p.IsDeleted))
            {
                throw new BusException(-1, $"分组名称“{request.Name}”已存在！");
            }
            //判断code是否存在
            if (await _repository.Select.AnyAsync(p => p.Code == request.Code && !p.IsDeleted))
            {
                throw new BusException(-1, $"分组编码“{request.Code}”已存在！");
            }
            //创建信息
            jobGroup.Id = _idGenerator.NewId();
            jobGroup.CreatedTime = DateTime.Now;
            jobGroup.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;

            await _repository.InsertAsync(jobGroup);
            return _mapper.Map<JobGroupItemResponse>(jobGroup);
        }

        public async Task Update(UpdateJobGroupRequest request)
        {
            //判断分组是否存在
            JobGroups jobGroup = await _repository.Where(p => p.Id == request.Id && !p.IsDeleted).FirstAsync();
            if (jobGroup == null)
            {
                throw new BusException(-1, $"修改的分组不存在");
            }
            //判断分组是否存在
            if (await _repository.Select.AnyAsync(p => p.Name == request.Name && !p.IsDeleted && p.Id != request.Id))
            {
                throw new BusException(-1, $"分组名称“{request.Name}”已存在！");
            }
            //判断编码是否重复
            if (await _repository.Select.AnyAsync(p => p.Code == request.Code && !p.IsDeleted && p.Id != request.Id))
            {
                throw new BusException(-1, $"分组编码“{request.Code}”已存在！");
            }
            jobGroup = request.MapTo(jobGroup);
            jobGroup.UpdatedTime = DateTime.Now;
            jobGroup.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;

            await _repository.UpdateAsync(jobGroup);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(-1, "没有要删除的条目");
            }
            List<JobGroups> allDelGroups = await _repository.Where(p => ids.Contains(p.Id))
                .NoTracking()
                .ToListAsync();
            foreach (var item in allDelGroups)
            {
                if (await _repository.Orm.Select<Jobs>().AnyAsync(p => p.GroupId == item.Id))
                {
                    throw new BusException(-1, $"分组“{item.Name}”正在使用，无法删除");
                }
            }
            await _repository.Where(p => ids.Contains(p.Id))
                .ToDelete()
                .ExecuteAffrowsAsync();
        }
    }
}
