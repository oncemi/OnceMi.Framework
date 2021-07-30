using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.AspNetCore.MQ;
using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.IService;
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
    /// 测试用
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V2)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IMessageQueneService _messageQuene;
        private readonly ILogger<TestController> _logger;
        private readonly ITestService _testService;

        public TestController(ILogger<TestController> logger
            , IMessageQueneService messageQuene
            , ITestService testService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageQuene = messageQuene ?? throw new ArgumentNullException(nameof(messageQuene));
            _testService = testService ?? throw new ArgumentNullException(nameof(testService));
        }

        [HttpGet]
        [Route(nameof(TestSyncProxyWithResult))]
        public int TestSyncProxyWithResult()
        {
            return _testService.TestSyncProxyWithResult();
        }

        [HttpGet]
        [Route(nameof(TestSyncProxyWithoutResult))]
        public void TestSyncProxyWithoutResult()
        {
            _testService.TestSyncProxyWithoutResult();
        }

        [HttpGet]
        [Route(nameof(TestAsyncProxyB))]
        public Task<int> TestAsyncProxyB()
        {
            return _testService.TestAsyncProxy();
        }

        [HttpGet]
        [Route(nameof(TestAsyncProxyA))]
        public async Task<int> TestAsyncProxyA()
        {
            return await _testService.TestAsyncProxy();
        }

        [HttpGet]
        [Route(nameof(TestTaskProxy))]
        public void TestTaskProxy()
        {
            _testService.TestTaskProxy();
        }

        // GET api/<ApiController>/5
        [HttpPost]
        public async Task Post(string name)
        {
            await _messageQuene.Publish<UploadFileRequest>(new UploadFileRequest()
            {
                FileName = "测试",
                FileOldName = "不晓得啦",
            });
        }

        [HttpPost]
        [Route("TestEnumToString")]
        public EnumTestModel TestEnumToString(EnumTestModel request)
        {
            return request;
        }
    }

    public class EnumTestModel
    {
        /// <summary>
        /// 类型
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MenuType? Type { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus? Status { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public UserGender Gender { get; set; }
    }
}
