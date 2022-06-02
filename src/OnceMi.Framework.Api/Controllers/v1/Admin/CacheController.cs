using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 缓存管理
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ILogger<CacheController> _logger;
        private readonly ICacheService _cacheService;

        public CacheController(ILogger<CacheController> logger
            , ICacheService cacheManageService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<CacheController>));
            _cacheService = cacheManageService ?? throw new ArgumentNullException(nameof(ICacheService));
        }

        /// <summary>
        /// 查询缓存Key
        /// </summary>
        /// <param name="queryString">模糊匹配</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public List<CacheKeyItemResponse> PageList([FromQuery] string queryString)
        {
            return _cacheService.GetCacheKeys(queryString);
        }

        /// <summary>
        /// 根据Key清除缓存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public DeleteCachesResponse Delete(DeleteCachesRequest request)
        {
            return _cacheService.DeleteCaches(request);
        }
    }
}
