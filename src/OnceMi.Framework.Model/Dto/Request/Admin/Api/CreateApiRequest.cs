using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(Apis))]
    public class CreateApiRequest : IRequest
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
        /// 版本
        /// </summary>
        [Required(ErrorMessage = "Api版本不能为空")]
        public string Version { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        [Required(ErrorMessage = "请求路径不能为空")]
        public string Path { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "Api名称不能为空")]
        public string Name { get; set; }

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
        public bool IsEnabled { get; set; } = true;
    }
}
