using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IUsersService : IBaseService
    {
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
        Task<UserItemResponse> Query(long id);

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
        Task UpdateUserStatus(UpdateUserStatusRequest request);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task Delete(List<long> ids);

        /// <summary>
        /// 获取用户头像
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<byte[]> GetAvatar(string name, int size = 100);
    }
}
