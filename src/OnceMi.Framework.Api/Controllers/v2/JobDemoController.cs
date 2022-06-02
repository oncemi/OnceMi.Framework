using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Enums;

namespace OnceMi.Framework.Api.Controllers.v2
{
    /// <summary>
    /// 定时任务Demo
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V2)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class JobDemoController : ControllerBase
    {
        private readonly IMessageQueneService _messageQuene;
        private readonly ILogger<JobDemoController> _logger;

        public JobDemoController(ILogger<JobDemoController> logger
            , IMessageQueneService messageQuene)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageQuene = messageQuene ?? throw new ArgumentNullException(nameof(messageQuene));
        }

        /// <summary>
        /// 执行定时任务
        /// </summary>
        /// <remarks>
        /// 定时任务接口需要加上[Job]特性
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Job]
        public async Task<object> Get([FromQuery] string id)
        {
            await Task.Delay(3000);

            return new
            {
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
    }

    public class TestRequestModel
    {
        public string Id { get; set; }
    }
}
