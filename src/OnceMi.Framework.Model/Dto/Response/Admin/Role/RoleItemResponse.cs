using OnceMi.Framework.Model.Attributes;
using System;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// API DTO
    /// </summary>
    [MapperTo(typeof(ICascaderResponse))]
    public class RoleItemResponse : ITreeResponse<RoleItemResponse>
    {
        /// <summary>
        /// 角色名称（唯一）
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 角色描述名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 组织Id
        /// </summary>
        public long OrganizeId { get; set; }

        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrganizeName { get; set; }

        /// <summary>
        ///排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 树标签
        /// </summary>
        public override string Label
        {
            get
            {
                return this.Name;
            }
            set
            {

            }
        }

        /// <summary>
        /// 创建者Id
        /// </summary>
        public long CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdatedTime { get; set; }
    }
}
