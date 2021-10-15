using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    public class LoginRequest : IRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(50)]
        [Required(ErrorMessage = "用户Id/名称不能为空")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [MaxLength(100)]
        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        [Required(ErrorMessage = "验证码不能为空")]
        [MaxLength(15)]
        public string Captcha { get; set; }
    }
}
