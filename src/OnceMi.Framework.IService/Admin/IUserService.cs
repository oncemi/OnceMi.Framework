using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IUserService : IBaseService
    {
        /// <summary>
        /// 查询用户状态
        /// </summary>
        /// <returns></returns>
        List<ISelectResponse<string>> GetUserStatus();

        /// <summary>
        /// 查询用户性别
        /// </summary>
        /// <returns></returns>
        List<ISelectResponse<string>> GetUserGender();

        /// <summary>
        /// 查询用户下拉
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<List<ISelectResponse<long>>> GetUserSelectList(string query);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="request"></param>
        /// <param name="onlyQueryEnabled"></param>
        /// <returns></returns>
        Task<IPageResponse<UserItemResponse>> Query(QueryUserPageRequest request, bool onlyQueryEnabled = false);

        /// <summary>
        /// 根据Id查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Users> Query(string inputInfo, bool isQueryEnabled = false);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserItemResponse> Insert(CreateUserRequest request);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task Update(UpdateUserRequest request);

        /// <summary>
        /// 更新用户状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task UpdateStatus(UpdateUserStatusRequest request);

        /// <summary>
        /// 更新密码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task UpdatePassword(UpdateUserPasswordRequest request);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task Delete(List<long> ids);

        /// <summary>
        /// 禁用用户Token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task DisableUserToken(long userId);

        /// <summary>
        /// 获取用户头像
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        byte[] GetAvatar(string name, int size);
    }
}
