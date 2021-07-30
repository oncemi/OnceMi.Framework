using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(Dictionaries))]
    public class CreateDictionaryRequest : IRequest
    {
        /// <summary>
        /// 字典父级
        /// </summary>
		private long? _parentId;

        /// <summary>
        /// 父Id
        /// </summary>
        public long? ParentId
        {
            get
            {
                if (_parentId == 0) return null;
                return _parentId;
            }
            set
            {
                _parentId = value;
            }
        }

        /// <summary>
        /// 字典名称
        /// </summary>
        [Required(ErrorMessage = "字典名称不能为空")]
        public string Name { get; set; }

        /// <summary>
        /// 字典编码
        /// </summary>
        [Required(ErrorMessage = "字典编码不能为空")]
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
		public bool Enabled { get; set; } = true;

        /// <summary>
        /// 排序
        /// </summary>
		public int Sort { get; set; } = 0;
    }
}
