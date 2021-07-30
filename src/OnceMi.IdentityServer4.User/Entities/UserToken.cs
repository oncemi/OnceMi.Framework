using FreeSql.DataAnnotations;

namespace OnceMi.IdentityServer4.User.Entities
{
    public class UserToken : IBaseEntity<long>
    {
        public long UserId { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Token { get; set; }
    }
}
