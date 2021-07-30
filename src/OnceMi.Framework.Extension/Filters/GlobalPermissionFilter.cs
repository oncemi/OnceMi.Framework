using FreeRedis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.Model;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Filters
{
    /// <summary>
    /// 全局授权过滤器
    /// </summary>
    public class GlobalPermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationService _authorization;
        private readonly RedisClient _redisClient;

        public GlobalPermissionFilter(IAuthorizationService authorization
            , RedisClient redisClient)
        {
            this._authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            this._redisClient = redisClient ?? throw new ArgumentNullException(nameof(redisClient));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string key = "123456";
            _redisClient.Set(AdminCacheKey.GetJobApiKey(key), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), TimeSpan.FromSeconds((8 + new Random().Next(5))));

            //检查是否为作业请求
            var isJobRequest = context.ActionDescriptor.EndpointMetadata?.Any(p => p is JobAttribute);
            if (isJobRequest == true)
            {
                //从redis中获取job key
                string jobKey = context.HttpContext.Request.Headers["JobKey"];
                if (string.IsNullOrEmpty(jobKey))
                {
                    context.Result = BadJobResult("请求被拒绝，此接口仅允许作业管理器请求。");
                    return;
                }
                string jobValue = _redisClient.Get(AdminCacheKey.GetJobApiKey(jobKey));
                if (string.IsNullOrEmpty(jobValue) || !DateTime.TryParse(jobValue, out DateTime _))
                {
                    context.Result = BadJobResult("请求被拒绝，此接口仅允许作业管理器请求。");
                    return;
                }
            }
            //检查是否有AllowAnonymous
            var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata?.Any(p => p is AllowAnonymousAttribute);
            if (hasAllowAnonymous == true)
            {
                return;
            }
            //内置授权验证（如果内置授权通过之后，就不进行下一步自定义授权）
            var authorizes = context.ActionDescriptor.EndpointMetadata?.Where(p => p is AuthorizeAttribute);
            foreach (var item in authorizes)
            {
                if (item == null || item is not AuthorizeAttribute) continue;
                var authorize = item as AuthorizeAttribute;
                if (!string.IsNullOrEmpty(authorize.Policy) || !string.IsNullOrEmpty(authorize.Roles))
                {
                    return;
                }
            }
            //自定义授权验证
            var result = await this._authorization.AuthorizeAsync(context.HttpContext.User, null, new GlobalPermissionRequirement()
            {
                ActionDescriptor = context.ActionDescriptor
            });
            if (!result.Succeeded)
            {
                context.Result = new ForbidResult();
            }
        }

        private IActionResult BadJobResult(string message)
        {
            return new JsonResult(new ResultObject<object>()
            {
                Code = (int)HttpStatusCode.BadRequest,
                Message = message,
                Data = null
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
            };
        }
    }
}
