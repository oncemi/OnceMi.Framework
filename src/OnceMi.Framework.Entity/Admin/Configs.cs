using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 配置项
    /// </summary>
    [Table(Name = nameof(Configs))]
    [Index("index_{TableName}_" + nameof(Key), nameof(Key), false)]
    public class Configs : IBaseEntity
    {
        /// <summary>
        /// 键名
        /// </summary>
        [MaxLength(120)]
        [Column(IsNullable = false)]
        public string Key { get; set; }

        /// <summary>
        /// 配置内容
        /// </summary>
        [Column(DbType = "text")]
        public string Content { get; set; } = "{}";

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 600, IsNullable = true)]
        public string Description { get; set; }
    }
}
