﻿using FreeSql.DataAnnotations;
using System.Collections.Generic;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 视图表
    /// </summary>
    [Table(Name = "sys_views")]
    public class View : IBaseEntity
    {
        [Column(IsNullable = true)]
        public long? ParentId { get; set; }

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
        /// 页面标题
        /// </summary>
        [Column(StringLength = 255, IsNullable = true)]
        public string PageTitle { get; set; }

        /// <summary>
        /// 路由
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Router { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        [Column(StringLength = 200, IsNullable = true)]
        public string Query { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 600, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        [Column(IsIgnore = true)]
        public List<View> Children { get; set; }
    }
}
