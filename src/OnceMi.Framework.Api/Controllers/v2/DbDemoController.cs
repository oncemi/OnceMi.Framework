using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;

namespace OnceMi.Framework.Api.Controllers.v2
{
    /// <summary>
    /// 测试接口
    /// </summary>
    /// <remarks>
    /// 用于测试一些功能，不需要可以直接删除
    /// </remarks>
    [ApiController]
    [ApiVersion(ApiVersions.V2)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DbDemoController : ControllerBase
    {
        private readonly IDemoService _service;
        private readonly IWebHostEnvironment _env;

        public DbDemoController(IDemoService service
            , IWebHostEnvironment env)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        /// <summary>
        /// 测试分库查询
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [Route("[action]")]
        [AllowAnonymous]
        public async Task<List<Configs>> GetAllConfigs()
        {
            if (!_env.IsDevelopment())
            {
                throw new Exception("非开发环境下，该功能无法使用！");
            }
            return await _service.GetAllConfigs();
        }

    }
}
