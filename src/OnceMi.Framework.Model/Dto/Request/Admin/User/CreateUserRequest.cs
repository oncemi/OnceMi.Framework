using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(Users))]
    public class CreateUserRequest : IRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名称不能为空")]
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
        [Required(ErrorMessage = "用户状态不能为空")]
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
        /// 更新密码，为空不更新
        /// </summary>
        [Required(ErrorMessage = "用户密码不能为空")]
        public string Password { get; set; }

        /// <summary>
        /// 密码是否被hash
        /// </summary>
        public bool PasswordHashed { get; set; } = false;

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
        /// 用户角色
        /// </summary>
        [Required(ErrorMessage = "用户角色不能为空")]
        public List<long> UserRoles { get; set; }

        /// <summary>
        /// 用户组织机构
        /// </summary>
        [Required(ErrorMessage = "用户组织机构不能为空")]
        public List<long> UserOrganizes { get; set; }

    }
}
