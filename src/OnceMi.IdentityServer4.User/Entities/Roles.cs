using FreeSql.DataAnnotations;
using System.Collections.Generic;

namespace OnceMi.IdentityServer4.User.Entities
{
    /// <summary>
    /// 角色表
    /// </summary>
    [Index("index_{TableName}_" + nameof(Name), nameof(Name), false)]
    [Index("index_{TableName}_" + nameof(Code), nameof(Code), false)]
    public class Roles : IBaseEntity<long>
    {
        [Column(Position = 2, IsNullable = true)]
        public long? ParentId { get; set; }

        /// <summary>
        /// 角色编码
        /// </summary>
        [Column(StringLength = 64, IsNullable = false)]
        public string Code { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        [Column(StringLength = 100, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 300)]
        public string Description { get; set; }

        /// <summary>
        /// 组织Id
        /// </summary>
        public long OrganizeId { get; set; }

        /// <summary>
        ///排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 本角色所属组织机构
        /// </summary>
        [Navigate(nameof(OrganizeId))]
        public Organizes Organize { get; set; }

        /// <summary>
        /// Users导航属性
        /// </summary>
        [Navigate(ManyToMany = typeof(UserRole))]
        public List<Users> Users { get; set; }

        [Column(IsIgnore = true)]
        public List<Roles> Children { get; set; }
    }
}
