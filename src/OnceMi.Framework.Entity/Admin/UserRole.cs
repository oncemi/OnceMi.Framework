using FreeSql.DataAnnotations;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 用户角色
    /// </summary>
    [Table(Name = "user_roles")]
    public class UserRole : IBaseEntity
    {
        [Column(IsNullable = false)]
        public long UserId { get; set; }

        [Column(IsNullable = false)]
        public long RoleId { get; set; }

        [Navigate(nameof(RoleId))]
        public Role Role { get; set; }

        [Navigate(nameof(UserId))]
        public UserInfo User { get; set; }
    }
}
