using System.ComponentModel;

namespace OnceMi.Framework.Model.Common
{
    /// <summary>
    /// 权限管理系统缓存KEY
    /// </summary>
    public static class AdminCacheKey
    {
        /// <summary>
        /// 验证码 admin:verify:code:guid
        /// </summary>
        [Description("验证码")]
        public const string VerifyCodeKey = "admin:verify:code:{0}";

        /// <summary>
        /// 角色 admin:role:角色主键
        /// </summary>
        [Description("角色")]
        public const string RoleItemKey = "admin:role:{0}";

        /// <summary>
        /// 角色 admin:role:角色主键
        /// </summary>
        [Description("用户信息")]
        public const string UserInfoKey = "admin:user:{0}:info";

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
        /// 获取验证码主键
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetVerifyCodeKey(string key)
        {
            return string.Format(VerifyCodeKey, key);
        }

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
        /// 获取角色主键
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static string GetRoleKey(long roleId)
        {
            return string.Format(RoleItemKey, roleId);
        }

        /// <summary>
        /// 获取用户信息主键
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetUserInfoKey(long userId)
        {
            return string.Format(UserInfoKey, userId);
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
