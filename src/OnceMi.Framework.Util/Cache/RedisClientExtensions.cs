using FreeRedis;
using OnceMi.Framework.Util.Json;
using System;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Cache
{
    public static class RedisClientExtensions
    {
        public static async Task<TResult> GetOrCreateAsync<TResult>(this RedisClient redis, string key, Func<Task<TResult>> function)
            where TResult : class
        {
            return await redis.GetOrCreateAsync(key, null, function);
        }

        public static async Task<TResult> GetOrCreateAsync<TResult>(this RedisClient redis, string key, TimeSpan? ts, Func<Task<TResult>> function)
            where TResult : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            TResult result;
            string resultStr = redis.Get(key);
            if (!string.IsNullOrEmpty(resultStr))
            {
                result = JsonUtil.DeserializeStringToObject<TResult>(resultStr);
                if (result != null)
                    return result;
            }
            result = await function();
            if (result == null)
                return default;
            if (ts == null || ts.Value.TotalMilliseconds == 0)
                redis.Set(key, JsonUtil.SerializeToString(result));
            else
                redis.Set(key, JsonUtil.SerializeToString(result), ts.Value);
            return result;
        }
    }
}
