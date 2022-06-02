using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Util.User;
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
        private readonly IRoleService _rolesService;
        private readonly IPermissionService _permissionService;
        private readonly IMenuService _menusService;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger
            , IHttpContextAccessor accessor
            , IRoleService rolesService
            , IPermissionService permissionService
            , IMenuService menusService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _rolesService = rolesService ?? throw new ArgumentNullException(nameof(rolesService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _menusService = menusService ?? throw new ArgumentNullException(nameof(menusService));
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
            //获取用户Id
            (string idStr, long? id) = context.User.GetSubject();
            if (id == null)
            {
                _logger.LogWarning($"{method} {path}|Can not get user id from request user identity.");
                context.Fail();
                return;
            }
            //判断角色是否存在
            List<long> roleIds = context.User.GetRoles();
            if (roleIds == null || roleIds.Count == 0)
            {
                _logger.LogWarning($"{method} {path}|Can not get user roles from request user identity, user id is {idStr}.");
                context.Fail();
                return;
            }
            //判断是否包含自定义授权
            if (requirement.ActionDescriptor.EndpointMetadata?.Any(p => p is SkipAuthorizationAttribute) == true)
            {
                context.Succeed(requirement);
                return;
            }
            //判断是否为开发人员
            if (await _rolesService.IsDeveloper(roleIds) != null)
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
            List<Menu> menus = await _menusService.Query(menuIds);
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
        private bool CompareMenu(Menu menu, string requestPath, string requestMethod)
        {
            if (string.IsNullOrEmpty(requestPath))
                return false;
            //请求方式
            if (!menu.Api.Method.Equals(requestMethod, StringComparison.OrdinalIgnoreCase))
                return false;
            //路径
            string[] paths = GetPaths(requestPath);
            string[] menuPaths = GetPaths(menu.Api.Path);
            if (paths.Length == 0 || paths.Length != menuPaths.Length)
                return false;
            for (int i = 0; i < menuPaths.Length; i++)
            {
                if (menu.Type == MenuType.Api
                    && IsParameter(menuPaths[i])
                    && CompareParameter(menuPaths[i], paths[i], menu.Api.ParameterDictionaries))
                    continue;
                if (!paths[i].Equals(menuPaths[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;

            #region Method

            bool IsParameter(string input) =>
                !string.IsNullOrEmpty(input) && input.Length > 2 && (input[0] == '{' && input[^1] == '}');

            string[] GetPaths(string path) =>
                string.IsNullOrWhiteSpace(path)
                ? new string[0]
                : path.Trim().Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            bool CompareParameter(string menuPathParam, string value, Dictionary<string, string> apiParams)
            {
                string paramName = menuPathParam[1..^1];
                if (!apiParams.ContainsKey(paramName))
                {
                    return false;
                }
                string type = apiParams[paramName];
                if (string.IsNullOrEmpty(type))
                {
                    return false;
                }
                switch (type.ToLower())
                {
                    case "integer":
                        return long.TryParse(value, out long _);
                    case "boolean":
                        return bool.TryParse(value, out bool _);
                    case "number":
                        return double.TryParse(value, out double _);
                    case "string":
                        {
                            //比如权限菜单路径为/a/b/{id}，且参数id为字符串，请求路径为 /a/b/c和/a/b/00000000-0000-0000-0000-000000000000
                            //此时无法方便c与00000000-0000-0000-0000-000000000000的区别，也就无法做权限控制了
                            //只有尽量不要使用string作为动态路径，本框架中均使用int64作为id。
                            return true;
                        }
                    case "object":
                    case "null":
                    case "empty":
                    case "array":
                    default:
                        return false;
                }
            }
            #endregion

        }

    }
}
