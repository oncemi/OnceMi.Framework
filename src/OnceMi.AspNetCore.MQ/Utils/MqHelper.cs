using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string name = $"APP-{appId}-{typeof(T).FullName}";
            return name;
        }
    }
}
