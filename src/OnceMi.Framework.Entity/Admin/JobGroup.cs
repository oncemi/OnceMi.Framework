using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 任务分组
    /// </summary>
    [Table(Name = "sys_job_groups")]
    public class JobGroup : IBaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Name { get; set; }


        /// <summary>
        /// 编码
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Code { get; set; }
    }
}
