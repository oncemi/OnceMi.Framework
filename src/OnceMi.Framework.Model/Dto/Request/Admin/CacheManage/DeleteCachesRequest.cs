using System.ComponentModel.DataAnnotations;

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
