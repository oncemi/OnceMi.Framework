using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 授权管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly ILogger<PermissionController> _logger;
        private readonly IPermissionService _service;

        public PermissionController(ILogger<PermissionController> logger
            , IPermissionService permissionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        /// <summary>
        /// 获取页面所需角色和菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<PermissionViewModel> Get()
        {
            return await _service.QueryPermissionList();
        }

        /// <summary>
        /// 获取角色启用的菜单ID
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns></returns>
        [HttpGet("{roleId}")]
        public async Task<List<long>> Get(long roleId)
        {
            return await _service.QueryRolePermissionList(roleId);
        }

        /// <summary>
        /// 获取用户角色操作权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(QueryUserRolePermission))]
        [NoPermission]
        public async Task<List<UserRolePermissionResponse>> QueryUserRolePermission()
        {
            List<long> ids = HttpContext.User.GetRoles()?.Distinct().ToList();
            if (ids == null || ids.Count == 0)
            {
                throw new BusException(-1, "获取用户角色失败！");
            }
            return await _service.QueryUserRolePermission(ids);
        }

        /// <summary>
        /// 修改授权
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task Put(UpdateRolePermissionRequest request)
        {
            await _service.UpdateRolePermissions(request);
        }
    }
}
