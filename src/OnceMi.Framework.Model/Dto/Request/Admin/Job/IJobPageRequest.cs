using OnceMi.Framework.Entity.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class IJobPageRequest : IPageRequest
    {
        /// <summary>
        /// 作业状态
        /// </summary>
        public JobStatus? Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        public string RequestMethod { get; set; }

        private long? _jobGroupId { get; set; }

        /// <summary>
        /// 作业分组
        /// </summary>
        public long? JobGroupId
        {
            get
            {
                return _jobGroupId;
            }
            set
            {
                if (value == null || value == 0)
                {
                    _jobGroupId = null;
                }
                _jobGroupId = value;
            }
        }
    }
}
