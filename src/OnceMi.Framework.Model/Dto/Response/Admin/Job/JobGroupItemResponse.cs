using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 作业分组DTO
    /// </summary>
    [MapperFrom(typeof(JobGroup))]
    public class JobGroupItemResponse : IResponse
    {
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        public long? CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedTime { get; set; }
    }
}
