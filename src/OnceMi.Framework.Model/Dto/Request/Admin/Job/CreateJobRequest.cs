using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(Jobs))]
    public class CreateJobRequest : IRequest
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "作业名称不能为空")]
        public string Name { get; set; }

        /// <summary>
        /// 分组Id
        /// </summary>
        [Required(ErrorMessage = "作业分组不能为空")]
        [Range(1, long.MaxValue, ErrorMessage = "作业分组不能为空")]
        public long GroupId { get; set; }

        /// <summary>
        /// 任务地址
        /// </summary>
        [Required(ErrorMessage = "任务地址不能为空")]
        public string Url { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        [Required(ErrorMessage = "请求方式不能为空")]
        public string RequestMethod { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        public string RequestHeader { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string RequestParam { get; set; }

        /// <summary>
        /// 执行计划
        /// </summary>
        [Required(ErrorMessage = "执行计划不能为空")]
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
        /// 通知发送策略
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NoticePolicy NoticePolicy { get; set; } = NoticePolicy.No;

        /// <summary>
        /// 通知发送角色组Id
        /// </summary>
        public long? NoticeRoleId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [MaxLength(300, ErrorMessage = "描述不能超过300个字符")]
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
