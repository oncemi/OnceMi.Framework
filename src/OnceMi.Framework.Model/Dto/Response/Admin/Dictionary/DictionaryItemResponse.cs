using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    [MapperFrom(typeof(Dictionary))]
    [MapperTo(typeof(ICascaderResponse))]
    public class DictionaryItemResponse : ITreeResponse<DictionaryItemResponse>
    {
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
		public bool IsEnabled { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
		public int Sort { get; set; }

        /// <summary>
        /// 树标签
        /// </summary>
        public override string Label
        {
            get
            {
                return this.Name;
            }
            set
            {

            }
        }
    }
}
