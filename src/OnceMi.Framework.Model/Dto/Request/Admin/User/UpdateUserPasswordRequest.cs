using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.IdentityServer4.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Dto
{
    public class UpdateUserPasswordRequest : IUpdateRequest
    {
        [Required(ErrorMessage = "旧密码不能为空")]
        public string oldPassword { get; set; }

        [Required(ErrorMessage = "新密码不能为空")]
        public string Password { get; set; }
    }
}
