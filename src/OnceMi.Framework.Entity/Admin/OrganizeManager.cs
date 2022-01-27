using FreeSql.DataAnnotations;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 组织机构管理人员（领导，分管领导）
    /// </summary>
    [Table(Name = "sys_organize_manager")]
    public class OrganizeManager : IBaseEntity
    {
        /// <summary>
        /// 组织Id
        /// </summary>
        public long OrganizeId { get; set; }

        /// <summary>
        /// 人员Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 领导类型
        /// </summary>
        public OrganizeManagerType ManagerType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 关联的组织结构
        /// </summary>
        [Navigate(nameof(OrganizeId))]
        public Organize Organize { get; set; }

        /// <summary>
        /// 关联的用户
        /// </summary>
        [Navigate(nameof(UserId))]
        public UserInfo User { get; set; }
    }

    public enum OrganizeManagerType
    {
        /// <summary>
        /// 部门领导
        /// </summary>
        DepartLeader = 1 << 0,

        /// <summary>
        /// 分管领导
        /// </summary>
        HeadLeader = 1 << 1,
    }
}
