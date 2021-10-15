using OnceMi.Framework.Entity.Admin;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
