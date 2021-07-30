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
    public class UpdateUserStatusRequest : IUpdateRequest
    {
        /// <summary>
        /// 用户状态
        /// </summary>
        [Required(ErrorMessage = "用户状态不能为空")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus Status { get; set; }

    }
}
