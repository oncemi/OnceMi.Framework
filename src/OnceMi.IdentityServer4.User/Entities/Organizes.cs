using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.IdentityServer4.User.Entities
{
    /// <summary>
    /// 组织机构
    /// </summary>
    [Table(Name = nameof(Organizes))]
    [Index("index_{TableName}_" + nameof(Name), nameof(Name), false)]
    [Index("index_{TableName}_" + nameof(Code), nameof(Code), false)]
    public class Organizes : IBaseEntity<long>
    {
        [Column(Position = 2, IsNullable = true)]
        public long? ParentId { get; set; }

        /// <summary>
        /// 组织机构名称
        /// </summary>
        [Column(StringLength = 64, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 组织机构编码
        /// </summary>
        [Column(StringLength = 64, IsNullable = false)]
        public string Code { get; set; }

        /// <summary>
        /// 组织类型
        /// </summary>
        public OrganizeType OrganizeType { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 部门领导
        /// </summary>
        [Column(IsIgnore = true)]
        public List<OrganizeManagers> DepartLeaders { get; set; }

        /// <summary>
        /// 分管领导
        /// </summary>
        [Column(IsIgnore = true)]
        public List<OrganizeManagers> HeadLeaders { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 300)]
        public string Description { get; set; }

        /// <summary>
        /// Users导航属性
        /// </summary>
        [Navigate(ManyToMany = typeof(UserOrganize))]
        public List<Users> Users { get; set; }

        /// <summary>
        /// Roles导航属性
        /// </summary>
        [Navigate(nameof(Entities.Roles.OrganizeId))]
        public List<Roles> Roles { get; set; }

        [Column(IsIgnore = true)]
        public List<Organizes> Children { get; set; }
    }

    public enum OrganizeType
    {
        /// <summary>
        /// 集团
        /// </summary>
        [Description("集团")]
        GroupCompany = 1 << 0,

        /// <summary>
        /// 总公司
        /// </summary>
        [Description("总公司")]
        Company = 1 << 1,

        /// <summary>
        /// 分公司
        /// </summary>
        [Description("分公司")]
        BranchCompany = 1 << 2,

        /// <summary>
        /// 办事处
        /// </summary>
        [Description("办事处")]
        Office = 1 << 3,

        /// <summary>
        /// 部门
        /// </summary>
        [Description("部门")]
        Department = 1 << 4,

        /// <summary>
        /// 工作组
        /// </summary>
        [Description("工作组")]
        WorkGroup = 1 << 5,
    }
}
