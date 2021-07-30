using Castle.DynamicProxy;
using FreeRedis;
using Microsoft.Extensions.Caching.Memory;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Aop
{
    public class CleanCacheAsyncInterceptor : ICleanCacheAsyncInterceptor
    {
        private readonly IMemoryCache _memoryCache;
        private readonly RedisClient _redisCache;

        public CleanCacheAsyncInterceptor(IMemoryCache memoryCache
            , RedisClient redisCache)
        {
            this._memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this._redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
        }

        /// <summary>
        /// 判断是否包含有CleanCache的属性
        /// </summary>
        /// <returns></returns>
        public (bool result, List<IAopAttribute> attrs) CanIntercept(IInvocation invocation)
        {
            List<IAopAttribute> attrs = invocation
                .MethodInvocationTarget
                .GetCustomAttributes(typeof(CleanCacheAttribute), false)
                ?.Select(p => p as IAopAttribute)
                ?.ToList();
            if (attrs == null || attrs.Count == 0)
            {
                return (false, null);
            }
            return (true, attrs);
        }

        /// <summary>
        /// 拦截异步方法 返回值为Task
        /// </summary>
        /// <param name="invocation"></param>
        public void InterceptAsynchronous(IInvocation invocation)
        {
            (bool result, List<IAopAttribute> attrs) = CanIntercept(invocation);
            if (result)
            {
                invocation.ReturnValue = InternalInterceptAsynchronous(invocation, attrs);
            }
            else
            {
                invocation.ReturnValue = InternalInterceptAsynchronous(invocation, null);
            }
        }

        /// <summary>
        /// 拦截异步方法 返回值为Task object
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="invocation"></param>
        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        /// <summary>
        /// 拦截同步执行的方法
        /// </summary>
        /// <param name="invocation"></param>
        public void InterceptSynchronous(IInvocation invocation)
        {
            InternalInterceptSynchronous(invocation);
            (bool result, List<IAopAttribute> attrs) = CanIntercept(invocation);
            if (result)
            {
                //清理缓存
                CleanCache(attrs);
            }
        }

        #region private

        private void InternalInterceptSynchronous(IInvocation invocation)
        {
            invocation.Proceed();
        }

        private async Task InternalInterceptAsynchronous(IInvocation invocation, List<IAopAttribute> attrs)
        {
            invocation.Proceed();

            //处理Task返回一个null值的情况会导致空指针
            if (invocation.ReturnValue != null)
            {
                await (Task)invocation.ReturnValue;
            }
            if (attrs != null && attrs.Count > 0)
            {
                //清理缓存
                CleanCache(attrs);
            }
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.Proceed();
            TResult returnValye = await (Task<TResult>)invocation.ReturnValue;

            (bool result, List<IAopAttribute> attrs) = CanIntercept(invocation);
            if (result)
            {
                //清除缓存
                CleanCache(attrs);
            }
            return returnValye;
        }

        private void CleanCache(List<IAopAttribute> attrs)
        {
            try
            {
                if (attrs == null) return;
                foreach (var item in attrs)
                {
                    CleanCacheAttribute cleanCacheAttribute = (CleanCacheAttribute)item;
                    switch (cleanCacheAttribute.CacheType)
                    {
                        case CacheType.MemoryCache:
                            {
                                _memoryCache.Remove(cleanCacheAttribute.Key);
                            }
                            break;
                        case CacheType.Redis:
                            {
                                _redisCache.Del(cleanCacheAttribute.Key);
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
