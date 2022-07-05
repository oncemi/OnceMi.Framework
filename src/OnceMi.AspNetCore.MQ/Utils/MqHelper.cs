using System;

namespace OnceMi.AspNetCore.MQ.Utils
{
    static class MqHelper
    {
        public static string CreateQueneNmae<T>(int appId) where T : class
        {
            if (appId < 0)
            {
                throw new ArgumentNullException("Error appid");
            }
            return $"APP-{appId}-{typeof(T).FullName}";
        }
    }
}
