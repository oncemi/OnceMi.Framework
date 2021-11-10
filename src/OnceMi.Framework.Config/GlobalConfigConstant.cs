using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnceMi.Framework.Config
{
    /// <summary>
    /// 全局静态配置
    /// </summary>
    public static class GlobalConfigConstant
    {
        /// <summary>
        /// 默认跨域策略名称
        /// </summary>
        public const string DefaultOriginsName = "DefaultCorsPolicy";

        /// <summary>
        /// 文件上传大小限制
        /// 单位：MB
        /// 限制：2048MB
        /// </summary>
        public const long FileUploadSizeLimit = 2048;

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
