using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 用户所属组织
    /// </summary>
    [Table(Name = "user_organizes")]
    public class UserOrganize : IBaseEntity
    {
        [Column(IsNullable = false)]
        public long UserId { get; set; }

        [Column(IsNullable = false)]
        public long OrganizeId { get; set; }

        [Navigate(nameof(OrganizeId))]
        public Organize Organize { get; set; }

        [Navigate(nameof(UserId))]
        public UserInfo User { get; set; }
    }
}
