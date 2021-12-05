using FreeSql.DataAnnotations;
using System.Collections.Generic;

namespace OnceMi.Framework.Entity.Admin
{
    [Table(Name = nameof(Apis))]
    public class Apis : IBaseEntity
    {
        [Column(IsNullable = true)]
        public long? ParentId { get; set; }

        /// <summary>
        /// 操作Id
        /// </summary>
        [Column(StringLength = 100, IsNullable = false)]
        public string OperationId { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string Version { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Path { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 操作码
        /// </summary>
        [Column(StringLength = 255, IsNullable = true)]
        public string Code { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 1000, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        [Column(StringLength = 10, IsNullable = true)]
        public string Method { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        [Column(StringLength = 1000, IsNullable = true)]
        public string Parameters { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 创建方式
        /// </summary>
        [Column(IsNullable = false)]
        public ApiCreateMethod CreateMethod { get; set; }

        /// <summary>
        /// 子条目
        /// </summary>
        [Column(IsIgnore = true)]
        public List<Apis> Children { get; set; }

        /// <summary>
        /// 参数字典
        /// </summary>
        [Column(IsIgnore = true)]
        public Dictionary<string,string> ParameterDictionaries { get; set; }
    }

    /// <summary>
    /// 创建方式
    /// </summary>
    public enum ApiCreateMethod
    {
        AutoSync = 1,
        Manual = 2,
    }
}
