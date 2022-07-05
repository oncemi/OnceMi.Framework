using OnceMi.Framework.Model.Dto;
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
        Task<LoginResponse> Login(LoginRequest request);

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        Task Logout();

        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<LoginResponse> RefreshToken(RefeshTokenRequest request);
    }
}
