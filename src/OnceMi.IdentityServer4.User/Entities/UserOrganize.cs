using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.IdentityServer4.User.Entities
{
    [Table(Name = nameof(UserOrganize))]
    public class UserOrganize : IBaseEntity<long>
    {
        [Column(IsNullable = false)]
        public long UserId { get; set; }

        [Column(IsNullable = false)]
        public long OrganizeId { get; set; }

        [Navigate(nameof(OrganizeId))]
        public Organizes Organize { get; set; }

        [Navigate(nameof(UserId))]
        public Users User { get; set; }
    }
}
