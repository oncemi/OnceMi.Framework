using System;
using System.Collections.Generic;
using System.Text;

namespace OnceMi.Framework.Config
{
    public class OSSConfigNode
    {
        public string Endpoint { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public string Region { get; set; }

        /// <summary>
        /// 是否启用Redis缓存临时URL
        /// </summary>
        public bool IsEnableCache { get; set; } = false;

        public bool IsEnableHttps { get; set; } = true;
    }
}
