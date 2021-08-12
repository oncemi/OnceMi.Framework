using FreeSql.DataAnnotations;
using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Entity.Admin
{
    [Table(Name = nameof(Jobs))]
    public class Jobs : IBaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 分组Id
        /// </summary>
        [Column(IsNullable = false)]
        public long GroupId { get; set; }

        /// <summary>
        /// 分组，字典
        /// </summary>
        [Navigate(nameof(GroupId))]
        public JobGroups Group { get; set; }

        /// <summary>
        /// 任务地址
        /// </summary>
        [Column(StringLength = 1000, IsNullable = false)]
        public string Url { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        [Column(StringLength = 10, IsNullable = false)]
        public string RequestMethod { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        [Column(DbType = "text", IsNullable = true)]
        public string RequestHeader { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        [Column(DbType = "text", IsNullable = true)]
        public string RequestParam { get; set; }

        /// <summary>
        /// 执行计划
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
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
        public NoticePolicy NoticePolicy { get; set; }

        /// <summary>
        /// 通知发送角色组Id
        /// </summary>
        [Column(IsNullable = true)]
        public long? NoticeRoleId { get; set; }

        /// <summary>
        /// 通知发送角色组
        /// </summary>
        [Navigate(nameof(NoticeRoleId))]
        public Roles NoticeRole { get; set; }

        /// <summary>
        /// 邮件通知地址，半角分号';'隔开
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int FireCount { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 600, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? LastFireTime { get; set; }

        /// <summary>
        /// 下次预计执行时间
        /// </summary>
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// 作业状态
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JobStatus Status { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 应用Id
        /// </summary>
        public int AppId { get; set; }

        [Column(IsIgnore = true)]
        public List<JobHistories> JobHistories { get; set; }
    }

    public enum NoticePolicy
    {
        /// <summary>
        /// 不发送
        /// </summary>
        [Description("不发送")]
        No = 1 << 0,

        /// <summary>
        /// 仅异常
        /// </summary>
        [Description("仅异常")]
        Error = 1 << 1,

        /// <summary>
        /// 全部发送
        /// </summary>
        [Description("全部发送")]
        All = 1 << 2,
    }

    public enum JobStatus
    {
        /// <summary>
        /// 已停止
        /// </summary>
        [Description("已停止")]
        Stopped = 1 << 0,

        /// <summary>
        /// 已暂停
        /// </summary>
        [Description("已暂停")]
        Paused = 1 << 1,

        /// <summary>
        /// 运行中
        /// </summary>
        [Description("运行中")]
        Running = 1 << 2,

        /// <summary>
        /// 等待中
        /// </summary>
        [Description("等待中")]
        Waiting = 1 << 3,
    }
}
