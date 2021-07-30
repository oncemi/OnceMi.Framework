using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class UpdateRolePermissionRequest : IRequest
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        [Required(ErrorMessage = "角色Id不能为空")]
        [Range(1, long.MaxValue, ErrorMessage = "角色Id不能为空")]
        public long RoleId { get; set; }

        /// <summary>
        /// 菜单权限
        /// </summary>
        [Required(ErrorMessage = "菜单权限列表不能为空")]
        public List<long> Permissions { get; set; }
    }
}
