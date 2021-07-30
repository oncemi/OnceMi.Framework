using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 按照名称清理缓存Key
    /// </summary>
    public class DeleteCachesRequest : IRequest
    {
        /// <summary>
        /// Key名称
        /// </summary>
        [Required(ErrorMessage = "Key不能为空")]
        public string Value { get; set; }
    }
}
