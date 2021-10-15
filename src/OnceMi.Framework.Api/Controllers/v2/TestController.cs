using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Util.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public class TestController : ControllerBase
    {
        private readonly IMessageQueneService _queneService;
        private readonly IConfigService _service;
        private readonly IWebHostEnvironment _env;
        private readonly IUserService _userService;

        public TestController(IMessageQueneService queneService
            , IConfigService service
            , IUserService userService
            , IWebHostEnvironment env)
        {
            _queneService = queneService;
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        /// <summary>
        /// 导出测试数据
        /// </summary>
        /// <remarks>
        /// 不需要可以直接注释掉
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportTestData()
        {
            if (!_env.IsDevelopment())
            {
                throw new Exception("非开发环境下，该功能无法使用！");
            }
            var stream = await _service.ExportTestData();
            return File(stream, "text/plain", "基础数据导出.txt");
        }

        /// <summary>
        /// 消息队列测试/Demo
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        [AllowAnonymous]
        public async Task<string> PublishTest(InputVal val)
        {
            await _queneService.Publish(new SubDemoModel()
            {
                Title = val.Message,
                Time = DateTime.Now,
                Span = (int)TimeSpan.FromSeconds(val.Time).TotalSeconds
            }, TimeSpan.FromSeconds(val.Time));

            return "发送成功";
        }

        [HttpGet]
        [Route("[action]")]
        [AllowAnonymous]
        public async Task<string> UpdatePwd()
        {
            List<long> ids = new List<long>()
            {
                68176475668423,68176475668424,78353217507397
            };
            foreach(var item in ids)
            {
                await _userService.UpdatePassword(new UpdateUserPasswordRequest()
                {
                    Id = item,
                    OldPassword = "1",
                    Password = Encrypt.SHA256("123456")
                });
            }
            return "OK";
        }
    }

    public class InputVal
    {
        public int Time { get; set; }

        public string Message { get; set; }
    }
}
