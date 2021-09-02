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

        /// <summary>
        /// 当前项目第一个命名空间
        /// </summary>
        public static string FirstNamespace
        {
            get
            {
                string mainDomainName = AppDomain.CurrentDomain.FriendlyName;
                if (string.IsNullOrEmpty(mainDomainName))
                {
                    throw new Exception("Get current domain friendly name.");
                }
                return mainDomainName.Split('.')[0];
            }
        }
    }
}
