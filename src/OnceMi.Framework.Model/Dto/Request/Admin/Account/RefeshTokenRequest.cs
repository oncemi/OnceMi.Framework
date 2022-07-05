using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    public class RefeshTokenRequest : IRequest
    {
        /// <summary>
        /// RefeshToken
        /// </summary>
        [Required(ErrorMessage = "RefeshToken不能为空")]
        public string Token { get; set; }
    }
}
