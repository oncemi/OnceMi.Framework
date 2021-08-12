using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    [MapperFrom(typeof(JobHistories))]
    public class JobHistoryItemResponse : IResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; }

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
        /// 执行状态
        /// </summary>
        public HistoryStatus Status { get; set; }

        public string StatusName
        {
            get
            {
                return Status.GetDescription();
            }
        }

        /// <summary>
        /// 执行结果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 下次触发时间
        /// </summary>
        public DateTime? NextFiredTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 任务阶段
        /// </summary>
        public HistoryStage Stage { get; set; }

        public string StageName
        {
            get
            {
                return Stage.GetDescription();
            }
        }
    }
}
