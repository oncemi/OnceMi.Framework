using FreeRedis;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.Extension.Helpers;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Util.User;
using System;
using System.Net;

namespace OnceMi.Framework.Extension.Filters
{
    /// <summary>
    /// 请求限流
    /// </summary>
    /// <remarks>
    /// 请求限流过滤器，登录用户每分钟请求次数
    /// 采用计数器算法和Redis，更好的可以采用令牌桶，但是令牌桶在分布式环境下实现起来有点麻烦。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequestLimitAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 限流次数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 限流时间段，单位：秒
        /// </summary>
        public int LimitSeconds
        {
            get
            {
                return _limitSeconds;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(LimitSeconds), "LimitTime can not less than 1.");
                }
                _limitSeconds = value;
            }
        }

        private int _limitSeconds = 0;
        private ILogger<RequestLimitAttribute> _logger;
        private RedisClient _redis;

        public RequestLimitAttribute()
        {
            this.Count = 100;
            this.LimitSeconds = 10;
        }

        public RequestLimitAttribute(int count, int limitTime)
        {
            this.Count = count;
            this.LimitSeconds = limitTime;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //获取必要的服务
            _logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequestLimitAttribute>>();
            _redis = context.HttpContext.RequestServices.GetRequiredService<RedisClient>();
            //获取请求的类名和方法名
            string controllerName = context.ActionDescriptor.RouteValues["Controller"];
            string actionName = context.ActionDescriptor.RouteValues["Action"];
            string method = context.HttpContext.Request.Method;
            //获取请求头中的token
            string token = context.HttpContext.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = FilterHelper.BuildResult(HttpStatusCode.Unauthorized);
                return;
            }
            if (!IsPassed(token, controllerName, actionName, method))
            {
                context.Result = FilterHelper.BuildResult(HttpStatusCode.Forbidden, "接口调用超出限制，请稍后再试");
                base.OnActionExecuting(context);
                return;
            }
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// 接口是否在限制次数范围内
        /// </summary>
        /// <param name="uniqueid">标识调用的key(accesstoken/token)</param>
        /// <param name="controller">服务</param>
        /// <param name="action">方法</param>
        /// <param name="method">请求方式</param>
        /// <returns></returns>
        private bool IsPassed(string uniqueid, string controller, string action, string method)
        {
            long max = this.Count;
            if (max == 0)
            {
                return true;
            }
            string key = GlobalCacheConstant.GetRequestLimitKey(uniqueid, controller, action, method);
            if (!_redis.Exists(key))
            {
                _redis.Set(key, "0", LimitSeconds);
            }
            long i = _redis.Incr(key);
            if (i > max)
            {
                if (i - 1 == max)
                {
                    long ttl = _redis.Ttl(key);
                    string message = @$"接口（{string.Concat(controller, " / ", action)}）{method.ToUpper()}请求，超过每{LimitSeconds}秒钟调用{this.Count}次限制，此消费方禁用该接口限制还剩{ttl}秒。";
                    _logger.LogWarning(message);
                }
                return false;
            }
            return true;
        }
    }
}
