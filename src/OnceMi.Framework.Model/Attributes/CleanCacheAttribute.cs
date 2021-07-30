using OnceMi.Framework.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Attributes
{
    /// <summary>
    /// 方法执行完成之后清理指定key的缓存
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CleanCacheAttribute : IAopAttribute
    {
        /// <summary>
        /// 缓存类型
        /// </summary>
        public CacheType CacheType { get; set; }

        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }

        public CleanCacheAttribute(CacheType cacheType, string key)
        {
            CacheType = cacheType;
            Key = key;
        }

        public CleanCacheAttribute()
        {

        }
    }
}
