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
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class DictionariesService : BaseService<Dictionaries, long>, IDictionariesService
    {
        private readonly IDictionariesRepository _repository;
        private readonly ILogger<DictionariesService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;

        public DictionariesService(IDictionariesRepository repository
            , ILogger<DictionariesService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accessor = accessor;
        }

        public async Task<int> QueryNextSortValue(long? parentId)
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

        public async Task<IPageResponse<DictionaryItemResponse>> Query(IPageRequest request)
        {
            IPageResponse<DictionaryItemResponse> response = new IPageResponse<DictionaryItemResponse>();
            bool isSearchQuery = false;
            Expression<Func<Dictionaries, bool>> exp = p => !p.IsDeleted;
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
            List<Dictionaries> allParents = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByModels)
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
                List<Dictionaries> removeDics = new List<Dictionaries>();
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
                List<Dictionaries> allDics = await _repository
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
                Dictionaries queryDic = await _repository
                    .Where(p => !p.IsDeleted && p.Code == request.Code)
                    .NoTracking()
                    .FirstAsync();
                if (queryDic == null)
                {
                    throw new BusException(-1, $"根据编码【{request.Code}】查询字典信息失败");
                }
                request.Id = queryDic.Id;
            }
            if (request.IncludeChild)
            {
                List<Dictionaries> allDics = await _repository
                    .Where(p => !p.IsDeleted)
                    .NoTracking()
                    .ToListAsync();
                if (allDics == null || allDics.Count == 0)
                {
                    throw new BusException(-1, $"查询字典信息失败");
                }
                Dictionaries queryDic = allDics.Where(p => p.Id == request.Id).FirstOrDefault();
                if (queryDic == null)
                    return null;

                GetQueryDictionaryChild(allDics, queryDic);
                DictionaryItemResponse result = _mapper.Map<DictionaryItemResponse>(queryDic);
                return result;
            }
            else
            {
                Dictionaries queryDic = await _repository
                    .Where(p => !p.IsDeleted && p.Id == request.Id)
                    .NoTracking()
                    .FirstAsync();
                DictionaryItemResponse result = _mapper.Map<DictionaryItemResponse>(queryDic);
                return result;
            }
        }

        public async Task<DictionaryItemResponse> Insert(CreateDictionaryRequest request)
        {
            Dictionaries dictionary = _mapper.Map<Dictionaries>(request);
            if (dictionary == null)
            {
                throw new Exception($"Map '{nameof(CreateDictionaryRequest)}' DTO to '{nameof(Dictionaries)}' entity failed.");
            }
            if ((dictionary.ParentId != null && dictionary.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == dictionary.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Code == dictionary.Code && p.ParentId == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前子目录下Code'{request.Code}'已存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Name == dictionary.Name && p.ParentId == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前子目录下Name'{request.Name}'已存在");
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
            Dictionaries dictionary = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (dictionary == null)
            {
                throw new BusException(-1, "修改的条目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Code == dictionary.Code && p.ParentId == request.ParentId && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前子目录下Code'{request.Code}'已存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Name == dictionary.Name && p.ParentId == request.ParentId && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前子目录下Name'{request.Name}'已存在");
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
                throw new BusException(-1, "没有要删除的条目");
            }
            List<Dictionaries> allDics = await _repository
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
            await _repository.Orm.Select<Dictionaries>()
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
        private void SearchDelDictionaries(List<Dictionaries> source, long id, List<long> dest)
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
            List<Dictionaries> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelDictionaries(source, citem.Id, dest);
            }
        }

        private void GetQueryDictionaryChild(List<Dictionaries> source, Dictionaries view, List<Dictionaries> removeDics = null)
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
