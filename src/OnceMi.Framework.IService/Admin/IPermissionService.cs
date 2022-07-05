using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IPermissionService : IBaseService<RolePermission, long>
    {
        /// <summary>
        /// 查询所有角色和所有菜单
        /// </summary>
        /// <returns></returns>
        Task<PermissionViewModel> QueryPermissionList();

        /// <summary>
        /// 查询角色的菜单权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<List<long>> QueryRolePermissionList(long roleId);

        /// <summary>
        /// 根据角色id查询角色权限
        /// </summary>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        Task<List<long>> QueryRolePermissionList(List<long> roleIds);

        /// <summary>
        /// 查询用户角色权操作权限
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<List<UserRolePermissionResponse>> QueryUserRolePermission(List<long> ids);

        /// <summary>
        /// 根据用户角色获取角色的菜单
        /// </summary>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        Task<List<UserMenuResponse>> QueryUserMenus(List<long> roleIds);

        /// <summary>
        /// 更新角色授权
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task UpdateRolePermissions(UpdateRolePermissionRequest request);
    }
}
