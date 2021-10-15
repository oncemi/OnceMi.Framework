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
        public const string SystemJobApiKey = "admin:sys:jobapis:{0}";

        /// <summary>
        /// 作业Key
        /// </summary>
        public const string SystemJobKey = "admin:sys:jobs:{0}";

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

        public static string GetJobKey(long id)
        {
            return string.Format(SystemJobKey, id);
        }
    }
}
