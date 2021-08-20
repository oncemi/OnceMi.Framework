using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using System;
using System.Threading.Tasks;

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _service;

        public AccountController(ILogger<AccountController> logger
            , IAccountService service)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        [AllowAnonymous]
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            return await _service.Authenticate(request);
        }

        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        [AllowAnonymous]
        public async Task<LoginResponse> Refesh(RefeshTokenRequest request)
        {
            return await _service.RefreshToken(request);
        }

        /// <summary>
        /// 撤销token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        [AllowAnonymous]
        public async Task RevokeToken(RevokeTokenRequest request)
        {
            await _service.RevokeToken(request);
        }
    }
}
