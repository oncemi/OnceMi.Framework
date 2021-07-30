using AutoMapper;
using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class ViewsService : BaseService<Views, long>, IViewsService
    {
        private readonly IViewsRepository _repository;
        private readonly ILogger<ViewsService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public ViewsService(IViewsRepository repository
            , ILogger<ViewsService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper
            , IMemoryCache cache) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accessor = accessor;
        }

        public async Task<IPageResponse<ViewItemResponse>> Query(IPageRequest request, bool onlyQueryEnabled = false)
        {
            IPageResponse<ViewItemResponse> response = new IPageResponse<ViewItemResponse>();
            bool isSearchQuery = false;
            Expression<Func<Views, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                isSearchQuery = true;
                exp = exp.And(p => p.Name.Contains(request.Search) || p.Path.Contains(request.Search));
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
            List<Views> allParents = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByModels)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allParents == null || allParents.Count == 0)
            {
                return new IPageResponse<ViewItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<ViewItemResponse>(),
                };
            }
            if (isSearchQuery)
            {
                List<Views> removeViews = new List<Views>();
                foreach (var item in allParents)
                {
                    GetQueryViewChild(allParents, item, removeViews);
                }
                if (removeViews.Count > 0)
                {
                    foreach (var item in removeViews)
                    {
                        allParents.Remove(item);
                    }
                }
            }
            else
            {
                Expression<Func<Views, bool>> allQueryExp = p => !p.IsDeleted && p.ParentId != null;
                if (onlyQueryEnabled)
                {
                    allQueryExp = allQueryExp.And(p => p.IsEnabled);
                }
                List<Views> allViews = await _repository
                    .Where(allQueryExp)
                    .NoTracking()
                    .ToListAsync();
                foreach (var item in allParents)
                {
                    GetQueryViewChild(allViews, item);
                }
            }
            return new IPageResponse<ViewItemResponse>()
            {
                Page = request.Page,
                Size = allParents.Count,
                Count = count,
                PageData = _mapper.Map<List<ViewItemResponse>>(allParents),
            };
        }

        public async Task<ViewItemResponse> Query(long id)
        {
            List<Views> allViews = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            Views queryView = allViews.Where(p => p.Id == id).FirstOrDefault();
            if (queryView == null)
                return null;

            GetQueryViewChild(allViews, queryView);
            ViewItemResponse result = _mapper.Map<ViewItemResponse>(queryView);
            return result;
        }

        [CleanCache(CacheType.MemoryCache, AdminCacheKey.SystemMenusKey)]
        public async Task<ViewItemResponse> Insert(CreateViewRequest request)
        {
            Views view = _mapper.Map<Views>(request);
            if (view == null)
            {
                throw new Exception($"Map '{nameof(CreateViewRequest)}' DTO to '{nameof(Views)}' entity failed.");
            }
            if ((view.ParentId != null && view.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == view.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, "父条目不存在");
            }
            view.ParentId = view.ParentId == 0 ? null : view.ParentId;
            view.Id = _idGenerator.NewId();
            view.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            view.CreatedTime = DateTime.Now;
            //保存
            var result = await _repository.InsertAsync(view);
            return _mapper.Map<ViewItemResponse>(result);
        }

        [CleanCache(CacheType.MemoryCache, AdminCacheKey.SystemMenusKey)]
        public async Task Update(UpdateViewRequest request)
        {
            Views view = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (view == null)
            {
                throw new BusException(-1, "修改的目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, "父条目不存在");
            }
            if (request.ParentId != null && request.ParentId != 0 && request.Id == request.ParentId)
            {
                throw new BusException(-1, "父条目不能为本身");
            }
            view = request.MapTo(view);
            view.ParentId = request.ParentId == 0 ? null : request.ParentId;
            view.UpdatedTime = DateTime.Now;
            view.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            await _repository.UpdateAsync(view);
        }

        [Transaction]
        [CleanCache(CacheType.MemoryCache, AdminCacheKey.SystemMenusKey)]
        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(-1, "没有要删除的条目");
            }
            List<Views> allViews = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            if (allViews == null || allViews.Count == 0)
            {
                return;
            }
            List<long> delIds = new List<long>();
            foreach (var item in ids)
            {
                SearchDelViews(allViews, item, delIds);
            }
            if (delIds == null || delIds.Count == 0)
            {
                return;
            }
            List<long> menuIds = await GetMenuIncludeViews(delIds);
            List<long> permissionIds = await GetPermissionIncludeMenus(menuIds);

            using (var uow = _repository.Orm.CreateUnitOfWork())
            {
                try
                {
                    if (delIds != null)
                        uow.Orm.Delete<Views>().Where(p => delIds.Contains(p.Id)).ExecuteAffrows();
                    if (menuIds != null)
                        uow.Orm.Delete<Views>().Where(p => menuIds.Contains(p.Id)).ExecuteAffrows();
                    if (permissionIds != null)
                        uow.Orm.Delete<RolePermissions>().Where(p => permissionIds.Contains(p.Id)).ExecuteAffrows();

                    uow.Commit();
                }
                catch (Exception ex)
                {
                    uow.Rollback();
                    _logger.LogError(ex, $"删除视图失败, {ex.Message}");
                    throw new BusException(-1, $"删除视图失败, {ex.Message}");
                }
            }
            //清除菜单缓存
            _cache.Remove(AdminCacheKey.SystemMenusKey);
        }

        /// <summary>
        /// 搜素要删除的父节点和子节点
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <param name="dest"></param>
        private void SearchDelViews(List<Views> source, long id, List<long> dest)
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
            List<Views> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelViews(source, citem.Id, dest);
            }
        }

        private void GetQueryViewChild(List<Views> source, Views view, List<Views> removeViews = null)
        {
            var childs = source.Where(p => p.ParentId == view.Id).ToList();
            if (childs == null || childs.Count == 0)
            {
                return;
            }
            view.Children = childs;
            if (removeViews != null)
            {
                removeViews.AddRange(childs);
            }
            foreach (var item in view.Children)
            {
                GetQueryViewChild(source, item);
            }
        }

        /// <summary>
        /// 获取要删除的菜单
        /// </summary>
        /// <param name="delIds"></param>
        /// <returns></returns>
        private async Task<List<long>> GetMenuIncludeViews(List<long> delIds)
        {
            if (delIds == null || delIds.Count == 0)
                return null;
            List<Menus> allMenus = await _repository.Orm.Select<Menus>()
                .Where(p => p.ViewId != null)
                .NoTracking()
                .ToListAsync();
            if (allMenus == null || allMenus.Count == 0)
                return null;
            //获取要删除的menuid
            List<long> delParentMenuIds = allMenus.Where(p => delIds.Contains(p.ViewId.Value))
                .Select(p => p.Id)
                .ToList();
            if (delParentMenuIds == null || delParentMenuIds.Count == 0)
                return null;
            List<long> delMenuIds = new List<long>();
            foreach (var item in delParentMenuIds)
            {
                SearchDelMenus(allMenus, item, delMenuIds);
            }
            return delMenuIds;
        }

        /// <summary>
        /// 获取要删除的菜单权限
        /// </summary>
        /// <param name="delIds"></param>
        /// <returns></returns>
        private async Task<List<long>> GetPermissionIncludeMenus(List<long> delIds)
        {
            if (delIds == null || delIds.Count == 0)
                return null;
            List<RolePermissions> allPermissions = await _repository.Orm.Select<RolePermissions>()
                .Where(p => delIds.Contains(p.MenuId))
                .NoTracking()
                .ToListAsync();
            if (allPermissions == null || allPermissions.Count == 0)
                return null;
            return allPermissions.Select(p => p.Id).ToList();
        }

        private void SearchDelMenus(List<Menus> source, long id, List<long> dest)
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
            List<Menus> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelMenus(source, citem.Id, dest);
            }
        }
    }
}
