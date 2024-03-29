﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exceptions;
using OnceMi.Framework.Util.Cache;
using OnceMi.Framework.Util.Json;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class PermissionService : BaseService<RolePermission, long>, IPermissionService
    {
        private readonly IPermissionRepository _repository;
        private readonly ILogger<PermissionService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly IMenuService _menuService;
        private readonly IRoleService _roleService;
        private readonly IRoleRepository _roleRepository;
        private readonly IMemoryCache _cache;

        public PermissionService(IPermissionRepository repository
            , ILogger<PermissionService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper
            , IMenuService menuService
            , IRoleService roleService
            , IRoleRepository roleRepository
            , IMemoryCache cache) : base(repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _accessor = accessor;
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<PermissionViewModel> QueryPermissionList()
        {
            PermissionViewModel vm = new PermissionViewModel();
            //查询角色
            var pageResult = await _roleService.Query(new IPageRequest()
            {
                Page = 1,
                Size = int.MaxValue,
            }, true);
            if (pageResult != null && pageResult.PageData != null && pageResult.PageData.Count() > 0)
            {
                vm.Roles = pageResult.PageData.ToList();
            }
            //查询全部菜单
            var menuResult = await _menuService.Query(new IPageRequest()
            {
                Page = 1,
                Size = int.MaxValue,
            }, true);
            if (menuResult != null && menuResult.PageData != null && menuResult.PageData.Count() > 0)
            {
                vm.Menus = menuResult.PageData.ToList();
            }
            return vm;
        }

        public async Task<List<long>> QueryRolePermissionList(long roleId)
        {
            List<RolePermission> allPermissions = await QueryRolePermissionsFromCache();
            List<RolePermission> permissions = allPermissions.Where(p => p.RoleId == roleId).ToList();
            if (permissions == null || permissions.Count == 0)
            {
                return new List<long>();
            }
            List<long> result = new List<long>();
            foreach (var item in permissions)
            {
                result.Add(item.MenuId);
            }
            return result;
        }

        public async Task<List<long>> QueryRolePermissionList(List<long> roleIds)
        {
            List<RolePermission> allPermissions = await QueryRolePermissionsFromCache();
            List<RolePermission> permissions = allPermissions.Where(p => roleIds.Contains(p.RoleId)).ToList();
            if (permissions == null || permissions.Count == 0)
            {
                return new List<long>();
            }
            List<long> result = new List<long>();
            foreach (var item in permissions)
            {
                if (result.Contains(item.MenuId))
                {
                    continue;
                }
                result.Add(item.MenuId);
            }
            return result;
        }

        public async Task<List<UserRolePermissionResponse>> QueryUserRolePermission(List<long> roleIds)
        {
            List<UserRolePermissionResponse> result = roleIds.Select(p => new UserRolePermissionResponse()
            {
                Id = p
            }).ToList();
            List<RolePermission> allPermissions = await QueryRolePermissionsFromCache();
            List<RolePermission> permissions = allPermissions.Where(p => roleIds.Contains(p.RoleId) && p.Menu != null && p.Menu.Type == MenuType.Api)
                .ToList();
            if (permissions == null || permissions.Count == 0)
            {
                return result;
            }
            foreach (var item in result)
            {
                item.Operation = permissions.Where(p => p.RoleId == item.Id).Select(p => p.Menu.Api.Code).Distinct().ToList();
            }
            return result;
        }

        public async Task<List<UserMenuResponse>> QueryUserMenus(List<long> roleIds)
        {
            List<long> menuIds = await QueryRolePermissionList(roleIds);
            if (menuIds == null || menuIds.Count == 0)
            {
                return new List<UserMenuResponse>();
            }
            List<Menu> allMenus = await _menuService.Query(menuIds);
            if (allMenus == null || allMenus.Count == 0)
            {
                return new List<UserMenuResponse>();
            }
            UserMenuResponse root = new UserMenuResponse()
            {
                Router = "root"
            };
            List<Menu> allParentsMenus = allMenus
                .Where(p => (p.ParentId == null || p.ParentId == 0) && p.Type != MenuType.Api)
                .OrderBy(p => p.Sort)
                .ThenBy(p => p.Id)
                .ToList();
            if (allParentsMenus != null && allParentsMenus.Count > 0)
            {
                foreach (var item in allParentsMenus)
                {
                    MapMenuToUserMenuResponse(allMenus, item, root);
                }
            }
            return new List<UserMenuResponse>()
            {
                root
            };
        }

        public async Task UpdateRolePermissions(UpdateRolePermissionRequest request)
        {
            if (!await _roleRepository.Select.AnyAsync(p => p.Id == request.RoleId && !p.IsDeleted && p.IsEnabled))
            {
                throw new BusException(ResultCode.PERM_UPDATE_ROLE_NOT_EXISTS, "更新的角色不存在或已被禁用。");
            }
            //获取全部的菜单
            List<Menu> allMenus = await _menuService.Where(p => !p.IsDeleted && p.IsEnabled).ToListAsync();
            if (allMenus == null)
            {
                allMenus = new List<Menu>();
            }
            List<Menu> permissionMenus = allMenus.Where(p => request.Permissions.Contains(p.Id)).ToList();
            List<long> distPermissions = new List<long>(request.Permissions);
            foreach (var item in permissionMenus)
            {
                FindMenuParents(allMenus, item, distPermissions);
            }

            if (distPermissions.Count == 0)
            {
                await _repository.DeleteAsync(p => p.RoleId == request.RoleId);
            }
            else
            {
                List<RolePermission> permissions = distPermissions.Select(p => new RolePermission()
                {
                    Id = _idGenerator.CreateId(),
                    RoleId = request.RoleId,
                    MenuId = p,
                    IsDeleted = false,
                    CreatedTime = DateTime.Now,
                    CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id
                }).ToList();
                await _repository.DeleteAsync(p => p.RoleId == request.RoleId);
                await _repository.InsertAsync(permissions);
            }
            //清空角色权限缓存
            _cache.Remove(GlobalCacheConstant.Key.RolePermissionsKey);
        }

        #region private

        private async Task<List<RolePermission>> QueryRolePermissionsFromCache()
        {
            //从缓存中取出所有菜单
            List<RolePermission> allPermissions = await _cache.GetOrCreateAsync(GlobalCacheConstant.Key.RolePermissionsKey, async (cache) =>
            {
                List<RolePermission> permissions = await _repository.Select
                    .LeftJoin(p => p.Menu.Id == p.MenuId)
                    .LeftJoin(p => p.Role.Id == p.RoleId)
                    .Where(p => !p.IsDeleted && !p.Menu.IsDeleted && p.Menu.IsEnabled && !p.Role.IsDeleted && p.Role.IsEnabled)
                    .OrderBy(p => p.Id)
                    .NoTracking()
                    .ToListAsync();
                if (permissions == null || permissions.Count == 0)
                {
                    return permissions;
                }
                List<View> views = await _repository.Orm.Select<View>().Where(p => !p.IsDeleted).NoTracking().ToListAsync();
                List<Api> apis = await _repository.Orm.Select<Api>().Where(p => !p.IsDeleted).NoTracking().ToListAsync();
                foreach (var item in permissions)
                {
                    if (item.Menu == null) continue;
                    if (item.Menu.Type == MenuType.Api && item.Menu.ApiId > 0 && apis != null && apis.Count > 0)
                    {
                        item.Menu.Api = apis.Where(p => p.Id == item.Menu.ApiId).FirstOrDefault();
                    }
                    if (item.Menu.Type == MenuType.View && item.Menu.ViewId > 0 && views != null && views.Count > 0)
                    {
                        item.Menu.View = views.Where(p => p.Id == item.Menu.ViewId).FirstOrDefault();
                    }
                }
                return permissions;
            });
            return allPermissions;
        }

        private void FindMenuParents(List<Menu> source, Menu target, List<long> dist)
        {
            if (!dist.Contains(target.Id))
            {
                dist.Add(target.Id);
            }
            if (target.ParentId != null && target.ParentId != 0)
            {
                Menu parent = source.Where(p => p.Id == target.ParentId).FirstOrDefault();
                if (parent == null) return;
                FindMenuParents(source, parent, dist);
            }
        }

        private void MapMenuToUserMenuResponse(List<Menu> source, Menu input, UserMenuResponse target)
        {
            if (input == null
                || (input.Type != MenuType.Group && input.Type != MenuType.View)
                || input.View == null)
            {
                return;
            }
            if (target.Children == null)
            {
                target.Children = new List<UserMenuResponse>();
            }
            UserMenuResponse nextTarget = new UserMenuResponse()
            {
                Router = input.View.Router,
                Path = input.View.Path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? null : input.View.Path,
                Name = input.Name,
                Icon = input.Icon,
                Invisible = input.IsHidden,
                Link = input.View.Path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? input.View.Path : null,
                Query = string.IsNullOrEmpty(input.View.Query) ? null : JsonUtil.DeserializeStringToObject<JsonElement>(input.View.Query),
                Page = string.IsNullOrEmpty(input.View.PageTitle) ? null : new MenuPageMetaResponse()
                {
                    Title = input.View.PageTitle,
                }
            };
            target.Children.Add(nextTarget);
            List<Menu> childs = source.Where(p => p.ParentId == input.Id && (p.Type == MenuType.Group || p.Type == MenuType.View))
                .OrderBy(p => p.Sort)
                .ThenBy(p => p.Id)
                .ToList();

            if (childs != null && childs.Count > 0)
            {
                foreach (var item in childs)
                {
                    MapMenuToUserMenuResponse(source, item, nextTarget);
                }
            }
        }
        #endregion
    }
}
