using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnceMi.IdentityServer4.User.Extensions
{
    public static class UserHelpers
    {
        /// <summary>
        /// 判断用户名是否包含特殊字符
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsUserName(string userName)
        {
            //用户名:英文和数字
            Regex checkUserName = new Regex("^[A-Za-z0-9]+$");
            var resultName = checkUserName.Match(userName);
            if (!resultName.Success)
            {
                return false;
            }
            return true;
        }
    }
}
