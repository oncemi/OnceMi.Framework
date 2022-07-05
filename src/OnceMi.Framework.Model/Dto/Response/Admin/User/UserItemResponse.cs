using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    /// <summary>
    /// API DTO
    /// </summary>
    [MapperFrom(typeof(UserInfo))]
    public class UserItemResponse : IResponse
    {
        public long Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus Status { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserGender Gender { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 电话号码是否已经确认
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 是否已经确认电子邮件
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        /// 电子邮件
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 创建用户ID
        /// </summary>
        public long? CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最后修改人Id
        /// </summary>
        public long? UpdatedUserId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? UpdatedTime { get; set; }

        public UserItemResponse CreateUser { get; set; }

        public UserItemResponse UpdateUser { get; set; }

        public List<RoleItemResponse> Roles;

        public List<OrganizeItemResponse> Organizes;
    }
}
