using System;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    public class SoftGrantRequest : IRequest
    {
        [Required(ErrorMessage = "机器码不能为空")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Token不能为空")]
        public string Token { get; set; }

        [Required(ErrorMessage = "版本号不能为空")]
        public string Version { get; set; }

        public DateTime RegistTime { get; set; } = DateTime.Now;
    }
}
