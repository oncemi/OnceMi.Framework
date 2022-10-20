using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.IdGenerator;
using OnceMi.Framework.Config;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IRepository;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Exceptions;
using OnceMi.Framework.Util.Cache;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _repository;
        private readonly ILogger<RoleService> _logger;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ConfigManager _config;
        private readonly IMenuService _menuService;

        public RoleService(IRoleRepository repository
            , ILogger<RoleService> logger
            , IIdGeneratorService idGenerator
            , IHttpContextAccessor accessor
            , IMapper mapper
            , IMemoryCache cache
            , ConfigManager config
            , IMenuService menuService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
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
            Expression<Func<Role, bool>> exp = p => !p.IsDeleted;
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
            List<Role> allParentRoles = await _repository.Select
                .Page(request.Page, request.Size)
                .OrderBy(request.OrderByParams)
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
                List<Role> removeRoles = new List<Role>();
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
                Expression<Func<Role, bool>> allQueryExp = p => !p.IsDeleted && p.ParentId != null;
                if (onlyQueryEnabled)
                {
                    allQueryExp = allQueryExp.And(p => p.IsEnabled);
                }
                List<Role> allRoles = await _repository.Select
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
            Expression<Func<Role, bool>> exp = p => ids.Contains(p.Id) && !p.IsDeleted;
            if (onlyQueryEnabled)
            {
                exp = exp.And(p => p.IsEnabled);
            }
            List<Role> roles = await _repository.Select
                .LeftJoin(p => p.Organize.Id == p.OrganizeId)
                .Where(exp)
                .NoTracking()
                .ToListAsync();
            return _mapper.Map<List<RoleItemResponse>>(roles);
        }

        public async Task<RoleItemResponse> Query(long id)
        {
            List<Role> allRoles = await _repository.Select
                .LeftJoin(p => p.Organize.Id == p.OrganizeId)
                .Where(p => !p.IsDeleted)
                .NoTracking()
                .ToListAsync();
            Role queryRole = allRoles.Where(p => p.Id == id).FirstOrDefault();
            if (queryRole == null)
                return null;

            GetQueryRoleChild(allRoles, queryRole);
            RoleItemResponse result = _mapper.Map<RoleItemResponse>(queryRole);
            return result;
        }

        public async Task<List<UserInfo>> QueryRoleUsers(long roleId)
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
            Role role = _mapper.Map<Role>(request);
            if (role == null)
            {
                throw new Exception($"Map '{nameof(CreateRoleRequest)}' DTO to '{nameof(Role)}' entity failed.");
            }
            if ((role.ParentId != null && role.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == role.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ROLE_PARENTS_NOT_EXISTS, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Code == role.Code && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ROLE_CODE_EXISTS, $"当前添加的角色编码‘{role.Code}’已存在");
            }
            if (!await _repository.Orm.Select<Organize>().AnyAsync(p => p.Id == request.OrganizeId && p.IsEnabled && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ROLE_ORGANIZES_NOT_EXISTS, $"所选组织不存在或已被停用");
            }

            role.ParentId = role.ParentId == 0 ? null : role.ParentId;
            role.Id = _idGenerator.CreateId();
            role.CreatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;
            role.CreatedTime = DateTime.Now;
            role.OrganizeId = request.OrganizeId;
            //保存
            var result = await _repository.InsertAsync(role);

            //清空角色权限缓存
            _cache.Remove(GlobalCacheConstant.Key.RolePermissionsKey);
            //清空开发人员角色缓存
            _cache.Remove(GlobalCacheConstant.GetRoleKey(_config.AppSettings.DeveloperRoleName));

            return _mapper.Map<RoleItemResponse>(result);
        }

        public async Task Update(UpdateRoleRequest request)
        {
            Role role = await _repository.Where(p => p.Id == request.Id).FirstAsync();
            if (role == null)
            {
                throw new BusException(ResultCode.ROLE_UPDATE_NOT_EXISTS, "修改的条目不存在");
            }
            if ((request.ParentId != null && request.ParentId != 0)
                && !await _repository.Select.AnyAsync(p => p.Id == request.ParentId && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ROLE_PARENTS_NOT_EXISTS, "父条目不存在");
            }
            if (await _repository.Select.AnyAsync(p => p.Code == request.Code && p.Id != request.Id && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ROLE_CODE_EXISTS, $"当前修改的角色编码‘{request.Code}’已存在");
            }
            if (!await _repository.Orm.Select<Organize>().AnyAsync(p => p.Id == request.OrganizeId && p.IsEnabled && !p.IsDeleted))
            {
                throw new BusException(ResultCode.ROLE_ORGANIZES_NOT_EXISTS, $"所选组织不存在或已被停用");
            }

            role = request.MapTo(role);
            role.ParentId = request.ParentId == 0 ? null : request.ParentId;
            role.UpdatedTime = DateTime.Now;
            role.UpdatedUserId = _accessor?.HttpContext?.User?.GetSubject().id;

            //清空角色权限缓存
            _cache.Remove(GlobalCacheConstant.Key.RolePermissionsKey);
            //清空开发人员角色缓存
            _cache.Remove(GlobalCacheConstant.GetRoleKey(_config.AppSettings.DeveloperRoleName));

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
                throw new BusException(ResultCode.ROLE_DELETE_NOT_EXISTS, "没有要删除的条目");
            }
            List<Role> allRoles = await _repository
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
                throw new BusException(ResultCode.ROLE_USERD, $"删除失败，角色{string.Join(',', inUseRoleNames)}已经被分配至用户");
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
            _cache.Remove(GlobalCacheConstant.Key.RolePermissionsKey);
            //清空开发人员角色缓存
            _cache.Remove(GlobalCacheConstant.GetRoleKey(_config.AppSettings.DeveloperRoleName));
        }

        #region private

        private async Task<Role> GetDeveloperRole()
        {
            Role role = await _cache.GetOrCreateAsync(GlobalCacheConstant.GetRoleKey(_config.AppSettings.DeveloperRoleName), async (cache) =>
            {
                Role developRole = await _repository.Select
                    .LeftJoin(p => p.Organize.Id == p.OrganizeId)
                    .Where(p => p.Code == _config.AppSettings.DeveloperRoleName && !p.IsDeleted && p.IsEnabled)
                    .FirstAsync();
                return developRole;
            });
            return role;
        }

        private void GetQueryRoleChild(List<Role> source, Role role, List<Role> removeRoles = null)
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
        private void SearchDelRoles(List<Role> source, long id, List<long> dest)
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
            List<Role> child = source.Where(p => p.ParentId == id).ToList();
            foreach (var citem in child)
            {
                SearchDelRoles(source, citem.Id, dest);
            }
        }

        #endregion
    }
}
