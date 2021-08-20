using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.IdentityServer4.User.Entities
{
    /// <summary>
    /// 组织机构管理人员（领导，分管领导）
    /// </summary>
    [Table(Name = nameof(OrganizeManagers))]
    public class OrganizeManagers : IBaseEntity<long>
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
        public Organizes Organize { get; set; }

        /// <summary>
        /// 关联的用户
        /// </summary>
        [Navigate(nameof(UserId))]
        public Users User { get; set; }
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
