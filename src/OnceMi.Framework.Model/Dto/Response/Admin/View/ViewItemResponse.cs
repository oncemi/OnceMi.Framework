using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;

namespace OnceMi.Framework.Model.Dto
{
    [MapperFrom(typeof(Views))]
    [MapperTo(typeof(ICascaderResponse))]
    public class ViewItemResponse : IResponse
    {
        public long Id { get; set; }

        public long? ParentId { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 路由
        /// </summary>
        public string Router { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// 页面标题
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

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
        public List<ViewItemResponse> Children { get; set; }
    }
}
