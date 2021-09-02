using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Config;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Extension.Filters;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Util.User;
using OnceMi.IdentityServer4.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Authorizations
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<GlobalPermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly IRolesService _rolesService;
        private readonly IPermissionService _permissionService;
        private readonly IMenusService _menusService;
        private readonly IUsersService _usersService;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger
            , IHttpContextAccessor accessor
            , IRolesService rolesService
            , IPermissionService permissionService
            , IMenusService menusService
            , IUsersService usersService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _rolesService = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _menusService = menusService ?? throw new ArgumentNullException(nameof(menusService));
            _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GlobalPermissionRequirement requirement)
        {
            //获取请求路径并判断请求路径
            string path = _accessor.HttpContext.Request.Path;
            string method = _accessor.HttpContext.Request.Method;
            //判断请求用户是否为空
            if (context.User == null)
            {
                _logger.LogWarning($"{method} {path}|Can not get user from request.");
                context.Fail();
                return;
            }

            //判断用户是否存在或被删除
            (string idStr, long? id) = context.User.GetSubject();
            if (id == null)
            {
                _logger.LogWarning($"{method} {path}|Can not get user id from request user identity.");
                context.Fail();
                return;
            }
            //可选从数据库判断用户状态，因为accessToken过期时间为1小时，用户被禁用后，最多一个小时候无法访问
            //UserItemResponse user = await _usersService.Query(id.Value);
            //if(user == null || user.Status != UserStatus.Enable)
            //{
            //    _logger.LogWarning($"{method} {path}|Can not get user info from db.");
            //    context.Fail();
            //    return;
            //}
            //判断角色是否存在
            List<long> roleIds = context.User.GetRoles();
            if (roleIds == null || roleIds.Count == 0)
            {
                _logger.LogWarning($"{method} {path}|Can not get user roles from request user identity, user id is {idStr}.");
                context.Fail();
                return;
            }
            //判断是否为开发人员
            if (await _rolesService.IsDeveloper(roleIds) != null)
            {
                context.Succeed(requirement);
                return;
            }
            //判断是否包含自定义授权
            if(requirement.ActionDescriptor.EndpointMetadata?.Any(p => p is NoAuthorizeAttribute) == true)
            {
                context.Succeed(requirement);
                return;
            }
            //获取角色授权的菜单
            List<long> menuIds = await _permissionService.QueryRolePermissionList(roleIds);
            if (menuIds == null || menuIds.Count == 0)
            {
                context.Fail();
                return;
            }
            List<Menus> menus = await _menusService.Query(menuIds);
            foreach (var item in menus)
            {
                if (item.Type == MenuType.Api
                    && item.Api != null
                    && !string.IsNullOrEmpty(item.Api.Method))
                {
                    if (CompareMenu(item, path, method))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
            context.Fail();
            return;
        }

        /// <summary>
        /// 比较请求的菜单和用户权限菜单是否相同
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="requestPath"></param>
        /// <param name="requestMethod"></param>
        /// <returns></returns>
        bool CompareMenu(Menus menu, string requestPath, string requestMethod)
        {
            if (string.IsNullOrEmpty(requestPath))
                return false;
            string[] paths = GetPaths(requestPath);
            if (!menu.Api.Method.Equals(requestMethod, StringComparison.OrdinalIgnoreCase))
                return false;
            string[] menuPaths = GetPaths(menu.Api.Path);
            if (paths.Length != menuPaths.Length)
                return false;
            for (int i = 0; i < menuPaths.Length; i++)
            {
                if (IsParameter(menuPaths[i]))
                    continue;
                if (!paths[i].Equals(menuPaths[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;

            bool IsParameter(string input)
                => !string.IsNullOrEmpty(input) && input.Length > 2 && (input[0] == '{' && input[^1] == '}');

            string[] GetPaths(string path)
                => path.Trim().Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
        }
    }
}
