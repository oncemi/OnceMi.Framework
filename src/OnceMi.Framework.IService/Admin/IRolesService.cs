using OnceMi.Framework.Model.Dto;
using OnceMi.IdentityServer4.User.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IRolesService : IBaseService
    {
        /// <summary>
        /// 获取下一排序顺序
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        ValueTask<int> QueryNextSortValue(long? parentId);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request">查询条件</param>
        /// <param name="onlyQueryEnabled">是否只查询启用的角色</param>
        /// <returns></returns>
        Task<IPageResponse<RoleItemResponse>> Query(IPageRequest request, bool onlyQueryEnabled = false);

        /// <summary>
        /// 根据Id查找角色信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<List<RoleItemResponse>> Query(List<long> ids, bool onlyQueryEnabled = false);

        /// <summary>
        /// 使用Id查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<RoleItemResponse> Query(long id);

        /// <summary>
        /// 查询角色组下的用户
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<List<Users>> QueryRoleUsers(long roleId);

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<RoleItemResponse> Insert(CreateRoleRequest request);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task Update(UpdateRoleRequest request);

        /// <summary>
        /// 删除（非物理删除）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task Delete(List<long> ids);

        /// <summary>
        /// 判断是否为开发人员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> IsDeveloper(long id);

        /// <summary>
        /// 判断是否为开发人员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<long?> IsDeveloper(List<long> ids);
    }
}
