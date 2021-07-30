using OnceMi.IdentityServer4.User.Entities;
using System;
using System.Text;

namespace OnceMi.IdentityServer4.User
{
    public static class UserPasswdExtension
    {
        /// <summary>
        /// 创建密码
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwd"></param>
        /// <returns></returns>
        public static string Create(this Users user, string passwd)
        {
            if (user == null || user.Id == 0)
                throw new Exception("用户信息不能为空！");
            if (string.IsNullOrEmpty(passwd))
                throw new Exception("用户密码不能为空！");
            string beforeEncrypt = $"{user.Id}_{user.UserName}_{passwd}_Ym^X73$Jy#8q5&sJ*6";
            return SHA256(beforeEncrypt);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwd"></param>
        /// <returns></returns>
        public static bool Authenticate(this Users user, string passwd)
        {
            if (user == null || string.IsNullOrEmpty(passwd))
                return false;
            return user.Password.Equals(Create(user, passwd));
        }

        private static string SHA256(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hash = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }

            return builder.ToString();
        }
    }
}
