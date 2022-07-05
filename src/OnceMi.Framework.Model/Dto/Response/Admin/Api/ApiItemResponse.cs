using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// API DTO
    /// </summary>
    [MapperFrom(typeof(Api))]
    [MapperTo(typeof(ICascaderResponse))]
    public class ApiItemResponse : IResponse
    {
        public long Id { get; set; }

        /// <summary>
        /// 父Id
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 操作Id
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 操作码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 创建者Id
        /// </summary>
        public long CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// 子条目
        /// </summary>
        public List<ApiItemResponse> Children { get; set; }
    }
}
