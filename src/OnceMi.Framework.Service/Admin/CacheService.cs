using FreeRedis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Util.Cache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnceMi.Framework.Service.Admin
{
    public class CacheService : ICacheService
    {
        private readonly ILogger<CacheService> _logger;
        private readonly RedisClient _redisCache;
        private readonly IMemoryCache _memoryCache;

        public CacheService(ILogger<CacheService> logger
            , RedisClient redisCache
            , IMemoryCache memoryCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<CacheService>));
            _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public DeleteCachesResponse DeleteCaches(DeleteCachesRequest request)
        {
            //remove memery cache
            long removeCount = RemoveMemeryCache(request.Value);
            //remove redis cache
            removeCount += RemoveRedisCache(request.Value);
            return new DeleteCachesResponse(removeCount);
        }

        public List<CacheKeyItemResponse> GetCacheKeys(string queryString)
        {
            List<CacheKeyItemResponse> result = new List<CacheKeyItemResponse>();
            var fields = typeof(AdminCacheKey).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var item in fields)
            {
                if (item.FieldType != typeof(string))
                    continue;
                if (item.GetCustomAttributes(typeof(DescriptionAttribute), false)?.FirstOrDefault() is not DescriptionAttribute description)
                    continue;
                string value = item.GetValue(null).ToString();
                if (string.IsNullOrEmpty(value))
                    continue;
                if (!string.IsNullOrEmpty(queryString) && !value.Contains(queryString))
                    continue;
                result.Add(new CacheKeyItemResponse()
                {
                    Name = item.Name,
                    Value = item.GetValue(null).ToString(),
                    Description = description.Description
                });
            }
            return result;
        }

        #region private

        private long RemoveRedisCache(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0;
            key = Regex.Replace(key, @"\{.*\}*", "*");
            var keys = _redisCache.Keys(key);
            if (keys == null || keys.Length == 0)
            {
                return 0;
            }
            return _redisCache.Del(keys);
        }

        private long RemoveMemeryCache(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0;
            key = Regex.Replace(key, @"\{.*\}*", ".");
            long removeCount = 0;
            IEnumerable<string> allKeys = _memoryCache.GetKeys<string>();
            if (allKeys != null && allKeys.Any())
            {
                foreach (var item in allKeys)
                {
                    if (Regex.IsMatch(item, key))
                    {
                        _memoryCache.Remove(item);
                        removeCount++;
                    }
                }
            }
            return removeCount;
        }

        #endregion
    }
}
