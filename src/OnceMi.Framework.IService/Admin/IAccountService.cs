using OnceMi.Framework.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.IService.Admin
{
    public interface IAccountService : IBaseService
    {
        /// <summary>
        /// 登录认证
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<LoginResponse> Authenticate(LoginRequest request);

        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<LoginResponse> RefreshToken(RefeshTokenRequest request);

        /// <summary>
        /// 撤销RefreshToken
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task RevokeToken(RevokeTokenRequest request);
    }
}
