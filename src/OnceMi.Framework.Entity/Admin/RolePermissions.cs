using FreeSql.DataAnnotations;
using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Admin
{
    [Table(Name = nameof(RolePermissions))]
    public class RolePermissions : IBaseEntity
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
        public Roles Role { get; set; }

        /// <summary>
        /// 菜单
        /// </summary>
        public Menus Menu { get; set; }
    }
}
