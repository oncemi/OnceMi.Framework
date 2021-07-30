using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service
{
    public class TestService : ITestService
    {
        private readonly ILogger<TestService> _logger;

        public TestService(ILogger<TestService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [CleanCache(CacheType.MemoryCache, AdminCacheKey.RolePermissionsKey)]
        public int TestSyncProxyWithResult()
        {
            return 1;
        }

        [CleanCache(CacheType.MemoryCache, AdminCacheKey.RolePermissionsKey)]
        public void TestSyncProxyWithoutResult()
        {
            _logger.LogInformation("TestSyncProxyWithoutResult");
        }

        [CleanCache(CacheType.MemoryCache, AdminCacheKey.RolePermissionsKey)]
        public async Task<int> TestAsyncProxy()
        {
            return await Task.FromResult(2);
        }

        [CleanCache(CacheType.MemoryCache, AdminCacheKey.RolePermissionsKey)]
        public Task TestTaskProxy()
        {
            _logger.LogInformation("TestTaskProxy");

            return Task.CompletedTask;
        }
    }
}
