using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Entity.Admin
{
    [Table(Name = nameof(Users))]
    [Index("index_{TableName}_" + nameof(UserName), nameof(UserName), true)]
    public class Users : IBaseEntity
    {
        /// <summary>
        /// 
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Column(StringLength = 60)]
        public string NickName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [Column(DbType = "text", IsNullable = true)]
        public string Avatar { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public UserGender Gender { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        [Description("出生日期")]
        [Column(IsNullable = true)]
        public DateTime? BirthDay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(StringLength = 255, IsNullable = true)]
        public string Address { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(StringLength = 64, IsNullable = true)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(StringLength = 255, IsNullable = true)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the provider name.
        /// </summary>
        [Column(StringLength = 255, IsNullable = true)]
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the provider subject identifier.
        /// </summary>
        [Column(IsNullable = true)]
        public long? ProviderId { get; set; }

        /// <summary>
        /// 用户Token
        /// </summary>
        [Navigate(nameof(Admin.UserToken.UserId))]
        public UserToken UserToken { get; set; }

        /// <summary>
        /// 所属角色
        /// 通过导航关系配置
        /// </summary>
        [Navigate(ManyToMany = typeof(UserRole))]
        public List<Roles> Roles { get; set; }

        /// <summary>
        /// 所属组织
        /// 通过导航关系配置
        /// </summary>
        [Navigate(ManyToMany = typeof(UserOrganize))]
        public List<Organizes> Organizes { get; set; }

        /// <summary>
        /// 创建用户
        /// </summary>
        [Navigate(nameof(CreatedUserId))]
        public Users CreateUser { get; set; }

        /// <summary>
        /// 更新用户
        /// </summary>
        [Navigate(nameof(UpdatedUserId))]
        public Users UpdateUser { get; set; }

        #region Method

        /// <summary>
        /// 创建密码
        /// </summary>
        /// <param name="passwd"></param>
        /// <returns></returns>
        public string CreatePassword(string passwd)
        {
            passwd = passwd?.Trim();
            if (string.IsNullOrEmpty(passwd))
                throw new Exception("用户密码不能为空！");
            var data = KeyDerivation.Pbkdf2(passwd, Encoding.UTF8.GetBytes($"A#oKaw39w##m@*39i^rP_{this.Id}"), KeyDerivationPrf.HMACSHA512, 10000, 64);
            string pwd = Convert.ToBase64String(data);
            return pwd;
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="passwd"></param>
        /// <returns></returns>
        public bool AuthenticatePassword(string passwd)
        {
            passwd = passwd?.Trim();
            if (string.IsNullOrEmpty(passwd))
                return false;
            return this.Password.Equals(CreatePassword(passwd));
        }

        #endregion
    }

    /// <summary>
    /// 用户性别
    /// </summary>
    public enum UserGender
    {
        [Description("未知")]
        Unknow = 0,

        [Description("男")]
        Male = 1,

        [Description("女")]
        Female = 2,

        [Description("其他")]
        All = 4,
    }

    public enum UserStatus
    {
        [Description("启用")]
        Enable = 1,

        [Description("禁用")]
        Disable = 2,
    }
}
