using FreeRedis;
using OnceMi.AspNetCore.OSS;
using OnceMi.Framework.Util.Json;
using System;

namespace OnceMi.Framework.Extension.Middlewares
{
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly RedisClient _cache;

        public RedisCacheProvider(RedisClient cache)
        {
            this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public T Get<T>(string key) where T : class
        {
            string val = _cache.Get(key);
            if (string.IsNullOrEmpty(val))
            {
                return default(T);
            }
            return JsonUtil.DeserializeStringToObject<T>(val);
        }

        public void Remove(string key)
        {
            _cache.Del(key);
        }

        public void Set<T>(string key, T value, TimeSpan ts) where T : class
        {
            if (value == null)
                return;
            _cache.Set(key, JsonUtil.SerializeToString(value), ts);
        }
    }
}
