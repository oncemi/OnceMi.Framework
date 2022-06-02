using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Article;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Article;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Article
{
    public class ArticleCategoryService : BaseService<ArticleCategory, long>, IArticleCategoryService
    {
        private readonly IArticleCategoryRepository _repository;
        private readonly ILogger<ArticleCategoryService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _accessor;

        public ArticleCategoryService(IArticleCategoryRepository repository
            , ILogger<ArticleCategoryService> logger
            , IIdGeneratorService idGenerator
            , IMapper mapper
            , IHttpContextAccessor accessor) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accessor = accessor;
        }

        public async Task<IPageResponse<ArticleCategoryResponse>> Query(IPageRequest request, bool onlyQueryEnabled = false)
        {
            IPageResponse<ArticleCategoryResponse> response = new IPageResponse<ArticleCategoryResponse>();
            bool isSearchQuery = false;
            Expression<Func<ArticleCategory, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                isSearchQuery = true;
                exp = exp.And(p => p.Name.Contains(request.Search));
            }
            if (!isSearchQuery)
            {
                exp = exp.And(p => p.ParentId == null);
            }
            if (onlyQueryEnabled)
            {
                exp = exp.And(p => p.IsEnabled);
            }
            //get count
            long count = await _repository.Where(exp).CountAsync();
            List<ArticleCategory> allParentCategoris = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByParams)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allParentCategoris == null || allParentCategoris.Count == 0)
            {
                return new IPageResponse<ArticleCategoryResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<ArticleCategoryResponse>(),
                };
            }
            if (isSearchQuery)
            {
                List<ArticleCategory> removeCategories = new List<ArticleCategory>();
                foreach (var item in allParentCategoris)
                {
                    GetQueryArticleCategoryChild(allParentCategoris, item, removeCategories);
                }
                if (removeCategories.Count > 0)
                {
                    foreach (var item in removeCategories)
                    {
                        allParentCategoris.Remove(item);
                    }
                }
            }
            else
            {
                Expression<Func<ArticleCategory, bool>> allQueryExp = p => !p.IsDeleted && p.ParentId != null;
                if (onlyQueryEnabled)
                {
                    allQueryExp = allQueryExp.And(p => p.IsEnabled);
                }
                List<ArticleCategory> allCategories = await _repository.Select
                    .Where(allQueryExp)
                    .NoTracking()
                    .ToListAsync();
                foreach (var item in allParentCategoris)
                {
                    GetQueryArticleCategoryChild(allCategories, item);
                }
            }
            return new IPageResponse<ArticleCategoryResponse>()
            {
                Page = request.Page,
                Size = allParentCategoris.Count,
                Count = count,
                PageData = _mapper.Map<List<ArticleCategoryResponse>>(allParentCategoris),
            };
        }

        public async Task<ArticleCategoryResponse> Query(long id)
        {
            List<ArticleCategory> allCategories = await _repository.Select
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            ArticleCategory queryCategory = allCategories.Where(p => p.Id == id).FirstOrDefault();
            if (queryCategory == null)
                return null;

            GetQueryArticleCategoryChild(allCategories, queryCategory);
            ArticleCategoryResponse result = _mapper.Map<ArticleCategoryResponse>(queryCategory);
            return result;
        }

        public async Task<ArticleCategoryResponse> Insert(CreateArticleCategoryRequest request)
        {
            ArticleCategory category = _mapper.Map<ArticleCategory>(request);
            if (category == null)
            {
                throw new Exception($"Map '{nameof(CreateArticleCategoryRequest)}' DTO to '{nameof(ArticleCategory)}' entity failed.");
            }
            if ((category.ParentId != null && category.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == category.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ARITICLECATEGORY_PARENTS_NOT_EXISTS, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Name == category.Name && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ARITICLECATEGORY_NAME_EXISTS, $"当前添加的分类名称‘{category.Name}’已存在");
            }
            category.ParentId = category.ParentId == 0 ? null : category.ParentId;
            category.Id = _idGenerator.NewId();
            category.IsLocked = false;
            category.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            category.CreatedTime = DateTime.Now;
            //保存
            var result = await _repository.InsertAsync(category);
            return _mapper.Map<ArticleCategoryResponse>(result);
        }

        public async Task Update(UpdateArticleCategoryRequest request)
        {
            ArticleCategory category = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (category == null)
            {
                throw new BusException(ResultCode.ARITICLECATEGORY_UPDATE_NOT_EXISTS, "修改的条目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ARITICLECATEGORY_PARENTS_NOT_EXISTS, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Name == request.Name && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ARITICLECATEGORY_NAME_EXISTS, $"当前添加的分类名称‘{category.Name}’已存在");
            }
            category = request.MapTo(category);
            category.ParentId = request.ParentId == 0 ? null : request.ParentId;
            category.UpdatedTime = DateTime.Now;
            category.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            await _repository.UpdateAsync(category);
        }

        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(ResultCode.ARITICLECATEGORY_DELETE_NOT_EXISTS, "没有要删除的条目");
            }
            List<ArticleCategory> allCategories = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            if (allCategories == null || allCategories.Count == 0)
            {
                return;
            }
            List<long> delIds = new List<long>();
            foreach (var item in ids)
            {
                SearchDelCategories(allCategories, item, delIds);
            }
            if (delIds == null || delIds.Count == 0)
            {
                return;
            }
            if (delIds == null || delIds.Count == 0)
                return;
            await _repository.Where(p => delIds.Contains(p.Id))
                .ToUpdate()
                .Set(p => p.IsDeleted, true)
                .Set(p => p.IsEnabled, false)
                .Set(p => p.UpdatedUserId, _accessor?.HttpContext?.User?.GetSubject().id)
                .ExecuteAffrowsAsync();
        }

        #region private

        private void GetQueryArticleCategoryChild(List<ArticleCategory> source, ArticleCategory category, List<ArticleCategory> removeCategories = null)
        {
            var childs = source.Where(p => p.ParentId == category.Id).ToList();
            if (childs == null || childs.Count == 0)
            {
                return;
            }
            category.Children = childs;
            if (removeCategories != null)
            {
                removeCategories.AddRange(childs);
            }
            foreach (var item in category.Children)
            {
                GetQueryArticleCategoryChild(source, item);
            }
        }

        /// <summary>
        /// 搜素要删除的父节点和子节点
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <param name="dest"></param>
        private void SearchDelCategories(List<ArticleCategory> source, long id, List<long> dest)
        {
            var item = source.Where(p => p.Id == id).FirstOrDefault();
            if (item == null)
            {
                return;
            }
            if (item.IsLocked)
            {
                throw new BusException(ResultCode.ARITICLECATEGORY_IS_LOCKED, "当前分组为锁定分组，无法被删除");
            }
            if (!dest.Contains(item.Id))
            {
                dest.Add(item.Id);
            }
            List<ArticleCategory> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelCategories(source, citem.Id, dest);
            }
        }

        #endregion
    }
}
