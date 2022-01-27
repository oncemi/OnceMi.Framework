using FreeSql.DataAnnotations;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 任务执行记录
    /// </summary>
    [Table(Name = "sys_job_history")]
    public class JobHistory : IBaseEntity
    {
        public long JobId { get; set; }

        /// <summary>
        /// 实际触发时间
        /// </summary>
        [Column(IsNullable = true)]
        public DateTime? FiredTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [Column(IsNullable = true)]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        [Column(IsNullable = true)]
        public int? Elapsed { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public HistoryStatus Status { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        [Column(DbType = "text", IsNullable = true)]
        public string Result { get; set; }

        /// <summary>
        /// 下次触发时间
        /// </summary>
        [Column(IsNullable = true)]
        public DateTime? NextFiredTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(StringLength = 255, IsNullable = true)]
        public string Remark { get; set; }

        /// <summary>
        /// 任务阶段
        /// </summary>
        public HistoryStage Stage { get; set; }

        //[Navigate(nameof(JobId))]
        //public Jobs Jobs { get; set; }
    }

    public enum HistoryStatus
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        Success = 1,

        /// <summary>
        /// 失败
        /// </summary>
        [Description("失败")]
        Failed = 2,

        /// <summary>
        /// 终止
        /// </summary>
        [Description("终止")]
        Vetoed = 3,
    }

    public enum HistoryStage
    {
        /// <summary>
        /// 启动中
        /// </summary>
        [Description("启动中")]
        Starting = 1 << 0,

        /// <summary>
        /// 已启动
        /// </summary>
        [Description("已启动")]
        Started = 1 << 1,

        /// <summary>
        /// 执行中
        /// </summary>
        [Description("执行中")]
        Running = 1 << 2,

        /// <summary>
        /// 停止中
        /// </summary>
        [Description("停止中")]
        Stopping = 1 << 3,

        /// <summary>
        /// 已停止
        /// </summary>
        [Description("已完成")]
        Stopped = 1 << 4,

        /// <summary>
        /// 初始化
        /// </summary>
        [Description("初始化")]
        Initialized = 1 << 5,
    }
}
