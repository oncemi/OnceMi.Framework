using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.IdentityServer4.User.Entities;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(Roles))]
    public class CreateRoleRequest : IRequest
    {
        private long? _parentId;

        /// <summary>
        /// 父Id
        /// </summary>
        public long? ParentId
        {
            get
            {
                if (_parentId == 0) return null;
                return _parentId;
            }
            set
            {
                _parentId = value;
            }
        }

        /// <summary>
        /// 角色名称（唯一）
        /// </summary>
        [Required(ErrorMessage = "角色编码不能为空")]
        public string Code { get; set; }

        /// <summary>
        /// 角色描述名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 组织Id
        /// </summary>
        [Required(ErrorMessage = "组织不能为空")]
        [Range(1, long.MaxValue, ErrorMessage = "组织不能为空")]
        public long OrganizeId { get; set; }

        /// <summary>
        ///排序
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "排序大小必须大于0")]
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
