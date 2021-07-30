using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// 更新请求
    /// </summary>
    public class IUpdateRequest : IRequest
    {
        [Required(ErrorMessage = "Id不能为空")]
        [Range(typeof(long), "1", "9223372036854775807", ErrorMessage = "Id不能为空")]
        public long Id { get; set; }
    }
}
