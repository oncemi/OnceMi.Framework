using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    [MapperFrom(typeof(JobHistories))]
    public class JobHistoryItemResponse
    {
        public long JobId { get; set; }

        /// <summary>
        /// 实际触发时间
        /// </summary>
        public DateTime? FiredTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public int? Elapsed { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 阶段
        /// </summary>
        public JobStage Stage { get; set; }
    }
}
