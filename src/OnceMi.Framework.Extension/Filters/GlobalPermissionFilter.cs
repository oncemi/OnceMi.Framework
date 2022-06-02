using FreeRedis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnceMi.Framework.Extension.Authorizations;
using OnceMi.Framework.Extension.Helpers;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Util.User;
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
        private readonly RedisClient _redis;

        public GlobalPermissionFilter(IAuthorizationService authorization
            , RedisClient redisClient)
        {
            this._authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            this._redis = redisClient ?? throw new ArgumentNullException(nameof(redisClient));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            //检查是否为作业请求
            var isJobRequest = context.ActionDescriptor.EndpointMetadata?.Any(p => p is JobAttribute);
            if (isJobRequest == true)
            {
                //从redis中获取job key
                string jobKey = context.HttpContext.Request.Headers["JobKey"];
                if (string.IsNullOrEmpty(jobKey))
                {
                    context.Result = FilterHelper.BuildResult(HttpStatusCode.BadRequest, "请求被拒绝，此接口仅允许作业管理器请求。");
                    return;
                }
                string jobValue = _redis.Get(GlobalCacheConstant.GetJobApiKey(jobKey));
                if (string.IsNullOrEmpty(jobValue) || !DateTime.TryParse(jobValue, out DateTime _))
                {
                    context.Result = FilterHelper.BuildResult(HttpStatusCode.BadRequest, "请求被拒绝，此接口仅允许作业管理器请求。");
                    return;
                }
            }
            //检查是否文件下载请求
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
            //JWT黑名单，用户登出或删除之后，有效期内的JWT会被存入redis中
            string jwt = context.HttpContext.GetToken();
            if (string.IsNullOrWhiteSpace(jwt))
            {
                context.Result = FilterHelper.BuildResult(HttpStatusCode.Unauthorized);
                return;
            }
            if (_redis.Exists(GlobalCacheConstant.GetJwtBlackListKey(jwt)))
            {
                context.Result = FilterHelper.BuildResult(HttpStatusCode.Unauthorized);
                return;
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
    }
}
