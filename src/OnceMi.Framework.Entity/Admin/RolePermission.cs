using FreeSql.DataAnnotations;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 角色权限表
    /// </summary>
    [Table(Name = "sys_role_permissions")]
    public class RolePermission : IBaseEntity
    {
        /// <summary>
        /// 角色Id
        /// </summary>
		public long RoleId { get; set; }

        /// <summary>
        /// 菜单Id
        /// </summary>
		public long MenuId { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        [Navigate(nameof(RoleId))]
        public Role Role { get; set; }

        /// <summary>
        /// 菜单
        /// </summary>
        [Navigate(nameof(MenuId))]
        public Menu Menu { get; set; }
    }
}
