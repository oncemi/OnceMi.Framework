using AutoMapper;
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
using OnceMi.Framework.Util.Cache;
using OnceMi.Framework.Util.Enum;
using OnceMi.Framework.Util.Json;
using OnceMi.Framework.Util.User;
using System.Linq.Expressions;

namespace OnceMi.Framework.Service.Admin
{
    public class MenuService : BaseService<Menu, long>, IMenuService
    {
        private readonly IMenuRepository _repository;
        private readonly ILogger<MenuService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public MenuService(IMenuRepository repository
            , ILogger<MenuService> logger
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

        public List<ISelectResponse<int>> QueryMenuTypes()
        {
            List<EnumModel> enumModels = EnumUtil.EnumToList<MenuType>();
            if (enumModels == null || enumModels.Count == 0)
            {
                return new List<ISelectResponse<int>>();
            }
            List<ISelectResponse<int>> result = enumModels
                .OrderBy(p => p.Value)
                .Select(p => new ISelectResponse<int>()
                {
                    Name = p.Description,
                    Value = p.Value,
                })
                .ToList();
            return result;
        }

        public async ValueTask<int> QueryNextSortValue(long? parentId)
        {
            parentId = parentId == 0 ? null : parentId;
            var maxValueMenuObj = await _repository.Where(p => p.ParentId == parentId && !p.IsDeleted)
                .OrderByDescending(p => p.Sort)
                .FirstAsync();
            if (maxValueMenuObj != null)
            {
                return maxValueMenuObj.Sort + 1;
            }
            return 1;
        }

        public async Task<IPageResponse<MenuItemResponse>> Query(IPageRequest request, bool onlyQueryEnabled = false)
        {
            IPageResponse<MenuItemResponse> response = new IPageResponse<MenuItemResponse>();
            //get from db or cache
            List<Menu> allMenus = await QueryMenusFromCache();
            if (allMenus == null || allMenus.Count == 0)
            {
                return new IPageResponse<MenuItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = 0,
                    PageData = new List<MenuItemResponse>(),
                };
            }
            bool isSearchQuery = !string.IsNullOrEmpty(request.Search);
            //get order result
            var selector = allMenus
                .Where(p => !p.IsDeleted)
                .OrderBy(request.OrderByModels);
            if (selector is IOrderedEnumerable<Menu>)
                selector = (selector as IOrderedEnumerable<Menu>).ThenBy(p => p.Sort);
            else
                selector = selector.OrderBy(p => p.Sort).ThenBy(p => p.Id);

            if (isSearchQuery)
                selector = selector.Where(p => p.Name.Contains(request.Search));
            else
                selector = selector.Where(p => p.ParentId == null);

            if (onlyQueryEnabled)
                selector = selector.Where(p => p.IsEnabled);

            //get count
            int count = selector.Count();
            //get page
            List<Menu> allParents = selector
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToList();
            if (allParents == null || allParents.Count == 0)
            {
                return new IPageResponse<MenuItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<MenuItemResponse>(),
                };
            }
            if (isSearchQuery)
            {
                List<Menu> removeMenus = new List<Menu>();
                foreach (var item in allParents)
                {
                    GetQueryMenuChild(allParents, item, removeMenus);
                }
                if (removeMenus.Count > 0)
                {
                    foreach (var item in removeMenus)
                    {
                        allParents.Remove(item);
                    }
                }
            }
            else
            {
                var childMenus = allMenus.Where(p => !p.IsDeleted && p.ParentId != null);
                if (onlyQueryEnabled)
                    childMenus = childMenus.Where(p => p.IsEnabled);

                foreach (var item in allParents)
                {
                    GetQueryMenuChild(childMenus.ToList(), item);
                }
            }
            return new IPageResponse<MenuItemResponse>()
            {
                Page = request.Page,
                Size = allParents.Count,
                Count = count,
                PageData = _mapper.Map<List<MenuItemResponse>>(allParents),
            };
        }

        public async Task<MenuItemResponse> Query(long id)
        {
            List<Menu> allMenus = await QueryMenusFromCache();
            Menu queryMenu = allMenus.Where(p => p.Id == id).FirstOrDefault();
            if (queryMenu == null)
                return null;

            GetQueryMenuChild(allMenus, queryMenu);
            MenuItemResponse result = _mapper.Map<MenuItemResponse>(queryMenu);
            return result;
        }

        public async Task<List<Menu>> Query(List<long> menuIds)
        {
            if (menuIds == null || menuIds.Count == 0)
            {
                return null;
            }
            //从缓存中取出所有菜单
            List<Menu> allMenus = await QueryMenusFromCache();
            if (allMenus == null)
            {
                return null;
            }
            List<Menu> menus = allMenus.Where(p => !p.IsDeleted && p.IsEnabled && menuIds.Contains(p.Id))
                .ToList();
            return menus;
        }

        [CleanCache(CacheType.MemoryCache, CacheConstant.SystemMenusKey)]
        [CleanCache(CacheType.MemoryCache, CacheConstant.RolePermissionsKey)]
        public async Task<MenuItemResponse> Insert(CreateMenuRequest request)
        {
            Menu menu = _mapper.Map<Menu>(request);
            if (menu == null)
            {
                throw new Exception($"Map '{nameof(CreateMenuRequest)}' DTO to '{nameof(Menu)}' entity failed.");
            }
            if ((menu.ParentId != null && menu.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == menu.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.MENU_PARENTS_NOT_EXISTS, "父目录不存在");
            }
            if (menu.ParentId != null && menu.ParentId != 0
                && await _repository.Select.AnyAsync(p => p.Id == menu.ParentId && p.Type == MenuType.Api))
            {
                throw new BusException(ResultCode.MENU_API_CANNOT_AS_PARENT, "接口不能作为父目录");
            }
            //更新sort
            if (request.Sort == 0)
            {
                Expression<Func<Menu, bool>> getSortExp = p => !p.IsDeleted;
                if (menu.ParentId != null)
                {
                    getSortExp = getSortExp.And(p => p.ParentId == menu.ParentId);
                }
                var lastSort = await _repository.Select
                    .Where(getSortExp)
                    .OrderByDescending(p => p.Sort)
                    .FirstAsync();
                if (lastSort == null)
                    menu.Sort = 1;
                else
                    menu.Sort = lastSort.Sort + 1;
            }
            //验证视图
            if (menu.Type == MenuType.View && menu.ViewId != null && menu.ViewId > 0)
            {
                View view = await _repository.Orm.Select<View>().Where(p => p.Id == menu.ViewId && !p.IsDeleted).FirstAsync();
                if (view == null)
                {
                    throw new BusException(ResultCode.MENU_VIEW_NOT_EXISTS, "指定的视图不存在");
                }
            }
            //验证Api
            if (menu.Type == MenuType.Api)
            {
                Api api = await _repository.Orm.Select<Api>().Where(p => p.Id == menu.ApiId).FirstAsync();
                if (api == null)
                {
                    throw new BusException(ResultCode.MENU_API_NOT_EXISTS, "指定的Api不存在");
                }
            }
            //set value
            menu.ParentId = menu.ParentId == 0 ? null : menu.ParentId;
            menu.Id = _idGenerator.NewId();
            menu.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            menu.CreatedTime = DateTime.Now;
            //保存
            var result = await _repository.InsertAsync(menu);
            if (result == null)
            {
                return null;
            }

            //清除菜单缓存
            _cache.Remove(CacheConstant.SystemMenusKey);
            //清空角色权限缓存
            _cache.Remove(CacheConstant.RolePermissionsKey);

            result = await _repository.Select
                .LeftJoin(p => p.Api.Id == p.ApiId)
                .LeftJoin(p => p.View.Id == p.ViewId)
                .Where(p => p.Id == menu.Id)
                .FirstAsync();
            return _mapper.Map<MenuItemResponse>(result);
        }

        [CleanCache(CacheType.MemoryCache, CacheConstant.SystemMenusKey)]
        [CleanCache(CacheType.MemoryCache, CacheConstant.RolePermissionsKey)]
        public async Task Update(UpdateMenuRequest request)
        {
            Menu menu = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (menu == null)
            {
                throw new BusException(ResultCode.MENU_NOT_EXISTS, "修改的条目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.MENU_PARENTS_NOT_EXISTS, "父目录不存在");
            }
            if (menu.ParentId != null && menu.ParentId != 0
                && await _repository.Select.AnyAsync(p => p.Id == menu.ParentId && p.Type == MenuType.Api))
            {
                throw new BusException(ResultCode.MENU_API_CANNOT_AS_PARENT, "接口不能作为父目录");
            }

            //set value
            menu = request.MapTo(menu);
            menu.ParentId = request.ParentId == 0 ? null : request.ParentId;
            menu.UpdatedTime = DateTime.Now;
            menu.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            //验证view是否正确
            if (menu.Type == MenuType.View && menu.ViewId != null && menu.ViewId > 0)
            {
                View view = await _repository.Orm.Select<View>().Where(p => p.Id == menu.ViewId && !p.IsDeleted).FirstAsync();
                if (view == null)
                {
                    throw new BusException(ResultCode.MENU_VIEW_NOT_EXISTS, "指定的视图不存在。");
                }
            }
            //验证api是否正确
            if (menu.Type == MenuType.Api)
            {
                Api api = await _repository.Orm.Select<Api>().Where(p => p.Id == menu.ApiId).FirstAsync();
                if (api == null)
                {
                    throw new BusException(ResultCode.MENU_API_NOT_EXISTS, "指定的Api不存在。");
                }
            }
            await _repository.UpdateAsync(menu);
        }

        [Transaction]
        [CleanCache(CacheType.MemoryCache, CacheConstant.SystemMenusKey)]
        [CleanCache(CacheType.MemoryCache, CacheConstant.RolePermissionsKey)]
        public async Task Delete(List<long> ids)
        {
            /*
             * 删除逻辑：
             * 数据：
             * 1、删除要删除的菜单节点，以及节点下的子节点；物理删除
             * 2、删除用户权限中使用的菜单
             * 缓存：
             * 4、移除菜单缓存
             * 5、移除角色权限缓存
             */

            if (ids == null || ids.Count == 0)
            {
                throw new BusException(ResultCode.MENU_DELETE_NOT_EXISTS, "没有要删除的条目");
            }
            List<Menu> allMenus = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            if (allMenus == null || allMenus.Count == 0)
            {
                return;
            }
            List<long> delIds = new List<long>();
            foreach (var item in ids)
            {
                SearchDelMenus(allMenus, item, delIds);
            }
            if (delIds == null || delIds.Count == 0)
            {
                return;
            }
            List<long> permissionIds = await GetPermissionIncludeMenus(delIds);
            if (delIds != null)
                await _repository.Orm.Delete<Menu>().Where(p => delIds.Contains(p.Id)).ExecuteAffrowsAsync();
            if (permissionIds != null)
                await _repository.Orm.Delete<RolePermission>().Where(p => permissionIds.Contains(p.Id)).ExecuteAffrowsAsync();
        }

        #region private

        private async Task<List<Menu>> QueryMenusFromCache()
        {
            //从缓存中取出所有菜单
            List<Menu> allMenus = await _cache.GetOrCreateAsync(CacheConstant.SystemMenusKey, async (cache) =>
             {
                 List<Menu> menus = await _repository
                     .Where(p => !p.IsDeleted)
                     .LeftJoin(p => p.Api.Id == p.ApiId)
                     .LeftJoin(p => p.View.Id == p.ViewId)
                     .OrderBy(p => p.Id)
                     .NoTracking()
                     .ToListAsync();
                 //格式化api 参数
                 if (menus != null && menus.Count > 0)
                 {
                     foreach (var menu in menus)
                     {
                         if (menu.Type != MenuType.Api
                         || menu.Api == null
                         || string.IsNullOrEmpty(menu.Api.Parameters)
                         || !JsonUtil.TryParse(menu.Api.Parameters, out Dictionary<string, string> paramDic))
                         {
                             continue;
                         }
                         menu.Api.ParameterDictionaries = paramDic;
                     }
                 }
                 return menus;
             });
            return allMenus;
        }


        /// <summary>
        /// 搜素要删除的父节点和子节点
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <param name="dest"></param>
        private void SearchDelMenus(List<Menu> source, long id, List<long> dest)
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
            List<Menu> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelMenus(source, citem.Id, dest);
            }
        }

        /// <summary>
        /// 获取当前菜单的子条目
        /// </summary>
        /// <param name="source"></param>
        /// <param name="menu"></param>
        /// <param name="removeMenus"></param>
        private void GetQueryMenuChild(List<Menu> source, Menu menu, List<Menu> removeMenus = null)
        {
            var childs = source.Where(p => p.ParentId == menu.Id).ToList();
            if (childs == null || childs.Count == 0)
            {
                return;
            }
            menu.Children = childs;
            if (removeMenus != null)
            {
                removeMenus.AddRange(childs);
            }
            foreach (var item in menu.Children)
            {
                GetQueryMenuChild(source, item);
            }
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
            List<RolePermission> allPermissions = await _repository.Orm.Select<RolePermission>()
                .Where(p => delIds.Contains(p.MenuId))
                .NoTracking()
                .ToListAsync();
            if (allPermissions == null || allPermissions.Count == 0)
                return null;
            return allPermissions.Select(p => p.Id).ToList();
        }

        #endregion
    }
}
