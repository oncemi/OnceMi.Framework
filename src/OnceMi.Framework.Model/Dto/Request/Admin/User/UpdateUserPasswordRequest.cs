using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    public class UpdateUserPasswordRequest : IUpdateRequest
    {
        [Required(ErrorMessage = "旧密码不能为空")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "新密码不能为空")]
        public string Password { get; set; }
    }
}
