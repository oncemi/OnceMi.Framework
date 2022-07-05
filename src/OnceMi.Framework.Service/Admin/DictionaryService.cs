using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exceptions;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class DictionaryService : BaseService<Dictionary, long>, IDictionaryService
    {
        private readonly IDictionaryRepository _repository;
        private readonly ILogger<DictionaryService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly RedisClient _redis;

        public DictionaryService(IDictionaryRepository repository
            , ILogger<DictionaryService> logger
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

        public async ValueTask<int> QueryNextSortValue(long? parentId)
        {
            using (var locker = _redis.Lock(GlobalCacheConstant.GetRedisLockKey(this.GetType().Namespace, (parentId == 0 ? 0 : parentId).ToString()), 60))
            {
                try
                {
                    parentId = parentId == 0 ? null : parentId;
                    var maxValueObj = await _repository.Where(p => p.ParentId == parentId && !p.IsDeleted)
                        .OrderByDescending(p => p.Sort)
                        .FirstAsync();
                    if (maxValueObj != null)
                    {
                        return maxValueObj.Sort + 1;
                    }
                    return 1;
                }
                finally
                {
                    locker?.Unlock();
                }
            }
        }

        public async Task<IPageResponse<DictionaryItemResponse>> Query(IPageRequest request)
        {
            IPageResponse<DictionaryItemResponse> response = new IPageResponse<DictionaryItemResponse>();
            bool isSearchQuery = false;
            Expression<Func<Dictionary, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                isSearchQuery = true;
                exp = exp.And(p => p.Name.Contains(request.Search) || p.Code.Contains(request.Search) || p.Value.Contains(request.Search));
            }
            if (!isSearchQuery)
            {
                exp = exp.And(p => p.ParentId == null);
            }
            //get count
            long count = await _repository.Where(exp).CountAsync();
            List<Dictionary> allParents = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByParams)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allParents == null || allParents.Count == 0)
            {
                return new IPageResponse<DictionaryItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<DictionaryItemResponse>(),
                };
            }
            if (isSearchQuery)
            {
                List<Dictionary> removeDics = new List<Dictionary>();
                foreach (var item in allParents)
                {
                    GetQueryDictionaryChild(allParents, item, removeDics);
                }
                if (removeDics.Count > 0)
                {
                    foreach (var item in removeDics)
                    {
                        allParents.Remove(item);
                    }
                }
            }
            else
            {
                List<Dictionary> allDics = await _repository
                    .Where(p => !p.IsDeleted && p.ParentId != null)
                    .NoTracking()
                    .ToListAsync();
                foreach (var item in allParents)
                {
                    GetQueryDictionaryChild(allDics, item);
                }
            }
            return new IPageResponse<DictionaryItemResponse>()
            {
                Page = request.Page,
                Size = allParents.Count,
                Count = count,
                PageData = _mapper.Map<List<DictionaryItemResponse>>(allParents),
            };
        }

        public async Task<DictionaryItemResponse> Query(DictionaryDetailRequest request)
        {
            if (!string.IsNullOrEmpty(request.Code) && (request.Id == null || request.Id == 0))
            {
                Dictionary queryDic = await _repository
                    .Where(p => !p.IsDeleted && p.Code == request.Code)
                    .NoTracking()
                    .FirstAsync();
                if (queryDic == null)
                {
                    throw new BusException(ResultCode.DIC_QUERY_BY_CODE_FAILED, $"根据编码【{request.Code}】查询字典信息失败");
                }
                request.Id = queryDic.Id;
            }
            if (request.IncludeChild)
            {
                List<Dictionary> allDics = await _repository
                    .Where(p => !p.IsDeleted)
                    .NoTracking()
                    .ToListAsync();
                if (allDics == null || allDics.Count == 0)
                {
                    throw new BusException(ResultCode.DIC_QUERY_FAILED, $"查询字典信息失败");
                }
                Dictionary queryDic = allDics.Where(p => p.Id == request.Id).FirstOrDefault();
                if (queryDic == null)
                    return null;

                GetQueryDictionaryChild(allDics, queryDic);
                DictionaryItemResponse result = _mapper.Map<DictionaryItemResponse>(queryDic);
                return result;
            }
            else
            {
                Dictionary queryDic = await _repository
                    .Where(p => !p.IsDeleted && p.Id == request.Id)
                    .NoTracking()
                    .FirstAsync();
                DictionaryItemResponse result = _mapper.Map<DictionaryItemResponse>(queryDic);
                return result;
            }
        }

        public async Task<DictionaryItemResponse> Insert(CreateDictionaryRequest request)
        {
            Dictionary dictionary = _mapper.Map<Dictionary>(request);
            if (dictionary == null)
            {
                throw new Exception($"Map '{nameof(CreateDictionaryRequest)}' DTO to '{nameof(Dictionary)}' entity failed.");
            }
            if ((dictionary.ParentId != null && dictionary.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == dictionary.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.DIC_PARENT_NOT_EXIST, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Code == dictionary.Code && p.ParentId == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.DIC_CODE_EXISTS_IN_PATH, $"当前子目录下编码为'{request.Code}'的字典已存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Name == dictionary.Name && p.ParentId == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.DIC_NAME_EXISTS_IN_PATH, $"当前子目录下名称为'{request.Name}'的字典已存在");
            }
            dictionary.ParentId = dictionary.ParentId == 0 ? null : dictionary.ParentId;
            //view.Id = _idGenerator.NewId();
            dictionary.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            dictionary.CreatedTime = DateTime.Now;
            //保存
            var result = await _repository.InsertAsync(dictionary);
            return _mapper.Map<DictionaryItemResponse>(result);
        }

        public async Task Update(UpdateDictionaryRequest request)
        {
            Dictionary dictionary = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (dictionary == null)
            {
                throw new BusException(ResultCode.DIC_UPDATE_ITEM_NOTEXISTS, "修改的条目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.DIC_PARENT_NOT_EXIST, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Code == dictionary.Code && p.ParentId == request.ParentId && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(ResultCode.DIC_CODE_EXISTS_IN_PATH, $"当前子目录下编码为'{request.Code}'的字典已存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Name == dictionary.Name && p.ParentId == request.ParentId && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(ResultCode.DIC_NAME_EXISTS_IN_PATH, $"当前子目录下名称为'{request.Name}'的字典已存在");
            }
            dictionary = request.MapTo(dictionary);
            dictionary.UpdatedTime = DateTime.Now;
            dictionary.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            await _repository.UpdateAsync(dictionary);
        }

        [Transaction]
        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(ResultCode.DIC_DELETE_NOT_EISTS, "没有要删除的条目");
            }
            List<Dictionary> allDics = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            if (allDics == null || allDics.Count == 0)
            {
                return;
            }
            List<long> delIds = new List<long>();
            foreach (var item in ids)
            {
                SearchDelDictionaries(allDics, item, delIds);
            }
            if (delIds == null || delIds.Count == 0)
            {
                return;
            }
            await _repository.Orm.Select<Dictionary>()
                .Where(p => delIds.Contains(p.Id))
                .ToDelete()
                .ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 搜素要删除的父节点和子节点
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <param name="dest"></param>
        private void SearchDelDictionaries(List<Dictionary> source, long id, List<long> dest)
        {
            var item = source.Where(p => p.Id == id).FirstOrDefault();
            if (item == null)
            {
                return;
            }
            if (!dest.Contains(item.Id))
            {
                dest.Add(item.Id);
            }
            List<Dictionary> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelDictionaries(source, citem.Id, dest);
            }
        }

        private void GetQueryDictionaryChild(List<Dictionary> source, Dictionary view, List<Dictionary> removeDics = null)
        {
            var childs = source.Where(p => p.ParentId == view.Id).ToList();
            if (childs == null || childs.Count == 0)
            {
                return;
            }
            view.Children = childs;
            if (removeDics != null)
            {
                removeDics.AddRange(childs);
            }
            foreach (var item in view.Children)
            {
                GetQueryDictionaryChild(source, item);
            }
        }
    }
}
