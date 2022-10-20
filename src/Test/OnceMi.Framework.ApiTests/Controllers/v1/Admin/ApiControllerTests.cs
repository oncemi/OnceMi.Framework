using OnceMi.Framework.Api.Controllers.v1.Admin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnceMi.Framework.ApiTests.Interface;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService.Admin;
using AutoMapper;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnceMi.Framework.Model.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace OnceMi.Framework.Api.Controllers.v1.Admin.Tests
{
    [TestClass()]
    public class ApiControllerTests : IBaseTest
    {
        private readonly ILogger<ApiControllerTests> _logger;
        private readonly IApiService _service;

        public ApiControllerTests()
        {
            _logger = Services.GetRequiredService<ILogger<ApiControllerTests>>();
            _service = Services.GetRequiredService<IApiService>();
        }

        [TestMethod()]
        public void ApiVersionsTest()
        {
            List<ISelectResponse<string>> list = _service.QueryApiVersions();
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void CascaderListTest()
        {
            Assert.Fail();
        }
    }
}