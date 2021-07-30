using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(Views))]
    public class CreateViewRequest : IRequest
    {
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
        /// 路径
        /// </summary>
        [StringLength(255, ErrorMessage = "视图路径长度不能超过255个字符")]
        public string Path { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        [StringLength(200, ErrorMessage = "参数长度不能超过200个字符")]
        public string Query { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "视图名称不能为空")]
        [StringLength(50, ErrorMessage = "视图名称长度不能超过50个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 路由
        /// </summary>
        [Required(ErrorMessage = "路由不能为空")]
        [StringLength(50, ErrorMessage = "视图名称长度不能超过50个字符")]
        public string Router { get; set; }

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
    }
}
