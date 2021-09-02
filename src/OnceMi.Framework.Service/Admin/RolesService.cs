using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Config;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Cache;
using OnceMi.Framework.Util.User;
using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class RolesService : IRolesService
    {
        private readonly IRolesRepository _repository;
        private readonly ILogger<RolesService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ConfigManager _config;
        private readonly IMenusService _menusService;

        public RolesService(IRolesRepository repository
            , ILogger<RolesService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper
            , IMemoryCache cache
            , ConfigManager config
            , IMenusService menusService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _menusService = menusService ?? throw new ArgumentNullException(nameof(menusService));
            _accessor = accessor;
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

        public async Task<IPageResponse<RoleItemResponse>> Query(IPageRequest request, bool onlyQueryEnabled = false)
        {
            IPageResponse<RoleItemResponse> response = new IPageResponse<RoleItemResponse>();
            bool isSearchQuery = false;
            Expression<Func<Roles, bool>> exp = p => !p.IsDeleted;
            if (!string.IsNullOrEmpty(request.Search))
            {
                isSearchQuery = true;
                exp = exp.And(p => p.Code.Contains(request.Search) || p.Name.Contains(request.Search));
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
            List<Roles> allParentRoles = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByModels)
                .OrderBy(p => p.Sort)
                .LeftJoin(p => p.Organize.Id == p.OrganizeId)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            if (allParentRoles == null || allParentRoles.Count == 0)
            {
                return new IPageResponse<RoleItemResponse>()
                {
                    Page = request.Page,
                    Size = 0,
                    Count = count,
                    PageData = new List<RoleItemResponse>(),
                };
            }
            if (isSearchQuery)
            {
                List<Roles> removeRoles = new List<Roles>();
                foreach (var item in allParentRoles)
                {
                    GetQueryRoleChild(allParentRoles, item, removeRoles);
                }
                if (removeRoles.Count > 0)
                {
                    foreach (var item in removeRoles)
                    {
                        allParentRoles.Remove(item);
                    }
                }
            }
            else
            {
                Expression<Func<Roles, bool>> allQueryExp = p => !p.IsDeleted && p.ParentId != null;
                if (onlyQueryEnabled)
                {
                    allQueryExp = allQueryExp.And(p => p.IsEnabled);
                }
                List<Roles> allRoles = await _repository.Select
                    .LeftJoin(p => p.Organize.Id == p.OrganizeId)
                    .Where(allQueryExp)
                    .NoTracking()
                    .ToListAsync();
                foreach (var item in allParentRoles)
                {
                    GetQueryRoleChild(allRoles, item);
                }
            }
            return new IPageResponse<RoleItemResponse>()
            {
                Page = request.Page,
                Size = allParentRoles.Count,
                Count = count,
                PageData = _mapper.Map<List<RoleItemResponse>>(allParentRoles),
            };
        }

        public async Task<List<RoleItemResponse>> Query(List<long> ids, bool onlyQueryEnabled = false)
        {
            Expression<Func<Roles, bool>> exp = p => ids.Contains(p.Id) && !p.IsDeleted;
            if (onlyQueryEnabled)
            {
                exp = exp.And(p => p.IsEnabled);
            }
            List<Roles> roles = await _repository.Select
                .LeftJoin(p => p.Organize.Id == p.OrganizeId)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            return _mapper.Map<List<RoleItemResponse>>(roles);
        }

        public async Task<RoleItemResponse> Query(long id)
        {
            List<Roles> allRoles = await _repository.Select
                .LeftJoin(p => p.Organize.Id == p.OrganizeId)
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            Roles queryRole = allRoles.Where(p => p.Id == id).FirstOrDefault();
            if (queryRole == null)
                return null;

            GetQueryRoleChild(allRoles, queryRole);
            RoleItemResponse result = _mapper.Map<RoleItemResponse>(queryRole);
            return result;
        }

        public async Task<List<Users>> QueryRoleUsers(long roleId)
        {
            var roleUsers = await _repository.Select
                .IncludeMany(p => p.Users)
                .Where(p => p.Id == roleId)
                .NoTracking()
                .FirstAsync();
            if (roleUsers == null || roleUsers.Users == null || roleUsers.Users.Count == 0)
            {
                return null;
            }
            return roleUsers.Users;
        }

        public async Task<RoleItemResponse> Insert(CreateRoleRequest request)
        {
            Roles role = _mapper.Map<Roles>(request);
            if (role == null)
            {
                throw new Exception($"Map '{nameof(CreateRoleRequest)}' DTO to '{nameof(Roles)}' entity failed.");
            }
            if ((role.ParentId != null && role.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == role.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Code == role.Code && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前添加的角色编码‘{role.Code}’已存在");
            }
            if (!await _repository.Orm.Select<Organizes>().AnyAsync(p => p.Id == request.OrganizeId && p.IsEnabled && !p.IsDeleted))
            {
                throw new BusException(-1, $"所选组织不存在或已被停用");
            }

            role.ParentId = role.ParentId == 0 ? null : role.ParentId;
            role.Id = _idGenerator.NewId();
            role.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            role.CreatedTime = DateTime.Now;
            role.OrganizeId = request.OrganizeId;
            //保存
            var result = await _repository.InsertAsync(role);

            //清空角色权限缓存
            _cache.Remove(AdminCacheKey.RolePermissionsKey);
            //清空开发人员角色缓存
            _cache.Remove(AdminCacheKey.GetRoleKey(_config.AppSettings.DeveloperRoleName));

            return _mapper.Map<RoleItemResponse>(result);
        }

        public async Task Update(UpdateRoleRequest request)
        {
            Roles role = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (role == null)
            {
                throw new BusException(-1, "修改的条目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(-1, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Code == request.Code && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(-1, $"当前修改的角色编码‘{request.Code}’已存在");
            }
            if (!await _repository.Orm.Select<Organizes>().AnyAsync(p => p.Id == request.OrganizeId && p.IsEnabled && !p.IsDeleted))
            {
                throw new BusException(-1, $"所选组织不存在或已被停用");
            }

            role = request.MapTo(role);
            role.ParentId = request.ParentId == 0 ? null : request.ParentId;
            role.UpdatedTime = DateTime.Now;
            role.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;

            //清空角色权限缓存
            _cache.Remove(AdminCacheKey.RolePermissionsKey);
            //清空开发人员角色缓存
            _cache.Remove(AdminCacheKey.GetRoleKey(_config.AppSettings.DeveloperRoleName));

            await _repository.UpdateAsync(role);
        }

        public async Task<bool> IsDeveloper(long id)
        {
            var role = await GetDeveloperRole();
            if (role == null)
                return false;
            if (id == role.Id)
                return true;
            return false;
        }

        public async Task<long?> IsDeveloper(List<long> ids)
        {
            var role = await GetDeveloperRole();
            if (role == null)
                return null;
            if (ids == null || ids.Count == 0)
                return null;
            if (ids.Contains(role.Id))
                return role.Id;
            return null;
        }

        /// <summary>
        /// 软删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task Delete(List<long> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(-1, "没有要删除的条目");
            }
            List<Roles> allRoles = await _repository
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            if (allRoles == null || allRoles.Count == 0)
                return;
            List<UserRole> userRoles = await _repository.Orm.Select<UserRole>()
                .Where(p => ids.Contains(p.RoleId))
                .ToListAsync();
            if (userRoles != null && userRoles.Count > 0)
            {
                //角色被用户占用
                List<string> inUseRoleNames = allRoles.Where(p => userRoles.Any(q => q.RoleId == p.Id))
                    .Select(p => p.Name)
                    .ToList();
                throw new BusException(-1, $"角色{string.Join(',', inUseRoleNames)}已经被分配至用户，无法删除");
            }
            List<long> delIds = new List<long>();
            foreach (var item in ids)
            {
                SearchDelRoles(allRoles, item, delIds);
            }
            if (delIds == null || delIds.Count == 0)
                return;
            await _repository.Where(p => delIds.Contains(p.Id))
                .ToUpdate()
                .Set(p => p.IsDeleted, true)
                .Set(p => p.IsEnabled, false)
                .Set(p => p.UpdatedUserId, _accessor?.HttpContext?.User?.GetSubject().id)
                .ExecuteAffrowsAsync();

            //清空角色权限缓存
            _cache.Remove(AdminCacheKey.RolePermissionsKey);
            //清空开发人员角色缓存
            _cache.Remove(AdminCacheKey.GetRoleKey(_config.AppSettings.DeveloperRoleName));
        }

        #region private

        private async Task<Roles> GetDeveloperRole()
        {
            Roles role = await _cache.GetOrCreateAsync(AdminCacheKey.GetRoleKey(_config.AppSettings.DeveloperRoleName), async (cache) =>
            {
                Roles developRole = await _repository.Select
                    .LeftJoin(p => p.Organize.Id == p.OrganizeId)
                    .Where(p => p.Code == _config.AppSettings.DeveloperRoleName && !p.IsDeleted && p.IsEnabled)
                    .FirstAsync();
                return developRole;
            });
            return role;
        }

        private void GetQueryRoleChild(List<Roles> source, Roles role, List<Roles> removeRoles = null)
        {
            var childs = source.Where(p => p.ParentId == role.Id).ToList();
            if (childs == null || childs.Count == 0)
            {
                return;
            }
            role.Children = childs;
            if (removeRoles != null)
            {
                removeRoles.AddRange(childs);
            }
            foreach (var item in role.Children)
            {
                GetQueryRoleChild(source, item);
            }
        }

        /// <summary>
        /// 搜素要删除的父节点和子节点
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <param name="dest"></param>
        private void SearchDelRoles(List<Roles> source, long id, List<long> dest)
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
            List<Roles> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelRoles(source, citem.Id, dest);
            }
        }

        #endregion
    }
}
