using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnceMi.Framework.Config
{
    public static class GlobalConstant
    {
        /// <summary>
        /// 默认跨域策略名称
        /// </summary>
        public const string DefaultOriginsName = "DefaultCorsPolicy";

        /// <summary>
        /// 默认json名称序列化策略
        /// 首字母小写
        /// </summary>
        public static JsonNamingPolicy DefaultJsonNamingPolicy = JsonNamingPolicy.CamelCase;
    }
}
