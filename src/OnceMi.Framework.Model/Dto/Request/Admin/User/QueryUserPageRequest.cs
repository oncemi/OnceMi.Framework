using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(Users))]
    public class QueryUserPageRequest : IPageRequest
    {
        /// <summary>
        /// 用户状态
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus? Status { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public long? RoleId { get; set; }

        /// <summary>
        /// 组织机构Id
        /// </summary>
        public long? OrganizeId { get; set; }
    }
}
