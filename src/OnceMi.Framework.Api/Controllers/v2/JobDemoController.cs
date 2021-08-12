using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.IdentityServer4.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Api.Controllers.v2
{
    /// <summary>
    /// Job Demo
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

        [HttpGet]
        [AllowAnonymous]
        [Job]
        public async Task<object> Get([FromQuery] string id)
        {
            await Task.Delay(10000);

            return new
            {
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        [HttpPost]
        [AllowAnonymous]
        [Job]
        public async Task<object> Post(TestRequestModel request)
        {
            await Task.Delay(1000);
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
