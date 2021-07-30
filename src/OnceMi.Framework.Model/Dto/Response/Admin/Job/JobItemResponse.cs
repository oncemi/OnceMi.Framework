using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Extensions;
using OnceMi.Framework.Util.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 菜单DTO
    /// </summary>
    [MapperFrom(typeof(Jobs))]
    public class JobItemResponse : IResponse
    {
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        public string GroupName
        {
            get
            {
                return this.Group == null ? null : this.Group.Name;
            }
        }

        public string GroupCode
        {
            get
            {
                return this.Group == null ? null : this.Group.Code;
            }
        }

        /// <summary>
        /// 分组，字典
        /// </summary>
        public JobGroups Group { get; set; }

        /// <summary>
        /// 任务地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        public string RequestMethod { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        public string RequestHeader { get; set; }

        public Dictionary<string, string> RequestHeaderDic { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string RequestParam { get; set; }

        public Dictionary<string, string> RequestParamDic { get; set; }

        /// <summary>
        /// 执行计划
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 邮件发送策略
        /// </summary>
        public EmailStrategy EmailStrategy { get; set; }

        /// <summary>
        /// 邮件通知地址，半角分号';'隔开
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int RunCount { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 作业状态
        /// </summary>
        public JobStatus Status { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        public List<JobHistoryItemResponse> JobHistories { get; set; }

        public long? CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedTime { get; set; }
    }
}
