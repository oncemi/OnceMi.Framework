using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class LoginRequest : IRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户Id/名称不能为空")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        [Required(ErrorMessage = "验证码不能为空")]
        public string Captcha { get; set; }
    }
}
