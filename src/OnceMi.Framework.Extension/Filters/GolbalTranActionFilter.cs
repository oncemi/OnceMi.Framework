using FreeRedis;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Filters
{
    /// <summary>
    /// 重复请求
    /// </summary>
    public class GolbalTranActionFilter : IActionFilter
    {
        private readonly ILogger<GolbalTranActionFilter> _logger;
        private readonly RedisClient _redis;

        public GolbalTranActionFilter(ILogger<GolbalTranActionFilter> logger
            , RedisClient redis)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            return;
        }
    }
}
