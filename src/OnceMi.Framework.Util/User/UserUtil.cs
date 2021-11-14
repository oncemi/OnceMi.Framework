using OnceMi.Framework.Util.Date;
using OnceMi.Framework.Util.Security;
using System;

namespace OnceMi.Framework.Util.User
{
    public class UserUtil
    {
        public static string PasswdAddSalt(string pwd, string salt)
        {
            if (string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(salt))
            {
                throw new Exception("Passwd or salt can not null.");
            }
            string newPwd = string.Format(salt, "I dont know what this", pwd);
            return MD5.MD5String(newPwd);
        }

        /// <summary>
        /// 创建token
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GenerateToken(string username)
        {
            string tokenKey = "oncemi.com";
            //生成规则
            string tokenStr = username + "_" + TimeUtil.Timestamp() + "_" + tokenKey;
            return MD5.MD5String(tokenStr);
        }
    }
}
