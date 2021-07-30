using FreeSql.DataAnnotations;

namespace OnceMi.IdentityServer4.User.Entities
{
    public class UserRole : IBaseEntity<long>
    {
        [Column(IsNullable = false)]
        public long UserId { get; set; }

        [Column(IsNullable = false)]
        public long RoleId { get; set; }

        [Navigate(nameof(RoleId))]
        public Roles Role { get; set; }

        [Navigate(nameof(UserId))]
        public Users User { get; set; }
    }
}
