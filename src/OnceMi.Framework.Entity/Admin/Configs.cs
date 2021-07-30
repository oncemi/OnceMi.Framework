using FreeSql.DataAnnotations;
using OnceMi.Framework.Util.Date;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Admin
{
    [Table(Name = nameof(Configs))]
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
    }
}
