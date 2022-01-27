using OnceMi.Framework.Entity.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Common
{
    public class JobExcuteResult
    {
        /// <summary>
        /// 执行结果
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// 实际触发时间
        /// </summary>
        public DateTime FiredTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public double? Elapsed { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// 执行异常
        /// </summary>
        public System.Exception Exception { get; set; }

        [JsonIgnore]
        public Job Job { get; set; }
    }
}
