﻿using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 字典表
    /// </summary>
    [Table(Name = "sys_dictionaries")]
    [Index("index_{TableName}_" + nameof(Name), nameof(Name), false)]
    public class Dictionary : IBaseEntity
    {
        /// <summary>
        /// 字典父级
        /// </summary>
        [Column(IsNullable = true)]
        public long? ParentId { get; set; }

        /// <summary>
        /// 字典名称
        /// </summary>
        [Column(StringLength = 100, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 字典编码
        /// </summary>
        [Column(StringLength = 100, IsNullable = true)]
        public string Code { get; set; }

        /// <summary>
        /// 字典值
        /// </summary>
        [Column(DbType = "text", IsNullable = true)]
        public string Value { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column(StringLength = 500, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// 字典状态
        /// </summary>
		public bool IsEnabled { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
		public int Sort { get; set; }

        /// <summary>
        /// 子条目
        /// </summary>
        [Column(IsIgnore = true)]
        public List<Dictionary> Children { get; set; }
    }
}
