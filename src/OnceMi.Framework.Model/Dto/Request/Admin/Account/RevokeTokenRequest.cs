using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    public class RevokeTokenRequest : IRequest
    {
        /// <summary>
        /// RefeshToken
        /// </summary>
        [Required(ErrorMessage = "RefeshToken不能为空")]
        public string Token { get; set; }
    }
}
