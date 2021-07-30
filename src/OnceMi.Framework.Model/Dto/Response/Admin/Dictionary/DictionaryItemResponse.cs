using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    [MapperFrom(typeof(Dictionaries))]
    [MapperTo(typeof(ICascaderResponse))]
    public class DictionaryItemResponse : IResponse
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 字典父级
        /// </summary>
		public long? ParentId { get; set; }

        /// <summary>
        /// 字典名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 字典编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 字典值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 启用
        /// </summary>
		public bool Enabled { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
		public int Sort { get; set; }

        /// <summary>
        /// 子条目
        /// </summary>
        public List<DictionaryItemResponse> Children { get; set; }
    }
}
