using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 流水号请求
    /// </summary>
    public class ITranRequest : IRequest
    {
        /// <summary>
        /// 当前页
        /// </summary>
        [Required(ErrorMessage = "查询页数不能为空")]
        [Range(0, 1000000, ErrorMessage = "页数查询范围只能在1-1000000之间")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        [Required(ErrorMessage = "每页查询条数不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = "查询条数必须大于0")]
        public int Size { get; set; } = 20;
    }
}
