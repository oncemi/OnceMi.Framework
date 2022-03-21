using OnceMi.Framework.Util.Security;
using System.ComponentModel;

namespace OnceMi.Framework.Model.Common
{
    /// <summary>
    /// 权限管理系统缓存KEY
    /// </summary>
    public static class CacheConstant
    {
        /// <summary>
        /// 角色 admin:role:角色主键
        /// </summary>
        [Description("角色")]
        public const string RoleItemKey = "admin:sys:role:{0}";

        /// <summary>
        /// 系统菜单
        /// </summary>
        [Description("系统菜单")]
        public const string SystemMenusKey = "admin:sys:menus:all";

        /// <summary>
        /// 角色权限缓存
        /// </summary>
        [Description("角色权限缓存")]
        public const string RolePermissionsKey = "admin:sys:rolepermissions:all";

        /// <summary>
        /// 作业管理请求Key
        /// </summary>
        [Description("作业管理请求Key")]
        public const string SystemJobApiKey = "admin:sys:jobapis:{0}";

        /// <summary>
        /// 作业条目信息缓存
        /// </summary>
        [Description("作业条目信息缓存（慎重清理，会导致作业状态不正确）")]
        public const string SystemJobKey = "admin:sys:jobs:{0}";

        /// <summary>
        /// 作业执行失败通知时间
        /// </summary>
        [Description("作业执行失败通知时间（用于避免频繁发送作业失败通知）")]
        public const string SystemJobNoticeTimeKey = "admin:sys:jobnotice:{0}";

        /// <summary>
        /// 配置项缓存
        /// </summary>
        [Description("配置项缓存")]
        public const string ConfigItemKey = "admin:sys:configs:{0}";

        /// <summary>
        /// 禁用或者删除用户,JWT禁用黑名单
        /// </summary>
        [Description("JWT禁用黑名单")]
        public const string JwtBlackListKey = "admin:sys:jwtblacklist:{0}";

        /// <summary>
        /// 用户请求限制KEY
        /// </summary>
        [Description("用户请求限制KEY")]
        public const string RequestLimitKey = "admin:sys:requestlimit:{0}:{1}";

        #region Methods

        /// <summary>
        /// 获取角色主键
        /// </summary>
        /// <param name="roleCode"></param>
        /// <returns></returns>
        public static string GetRoleKey(string roleCode)
        {
            return string.Format(RoleItemKey, roleCode);
        }

        /// <summary>
        /// 获取作业请求key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetJobApiKey(string key)
        {
            return string.Format(SystemJobApiKey, key);
        }

        /// <summary>
        /// 获取作业key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetJobKey(long id)
        {
            return string.Format(SystemJobKey, id);
        }

        /// <summary>
        /// 获取作业执行失败通知key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetJobNoticeTimeKey(long id)
        {
            return string.Format(SystemJobNoticeTimeKey, id);
        }

        /// <summary>
        /// 获取配置项的缓存keu
        /// </summary>
        /// <param name="key">配置项key</param>
        /// <returns></returns>
        public static string GetConfigKey(string key)
        {
            return string.Format(ConfigItemKey, key);
        }

        /// <summary>
        /// JWT禁用黑名单
        /// </summary>
        /// <param name="jwt">传入的用户jwt</param>
        /// <returns></returns>
        public static string GetJwtBlackListKey(string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                throw new ArgumentNullException(nameof(jwt));
            }
            return string.Format(JwtBlackListKey, SHA.SHA256(jwt));
        }

        public static string GetRequestLimitKey(string jwt, string controller, string action, string method)
        {
            if (string.IsNullOrEmpty(jwt) || string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException();
            }
            return string.Format(RequestLimitKey, SHA.SHA256(jwt), $"{controller}_{action}_{method}");
        }

        #endregion
    }
}
