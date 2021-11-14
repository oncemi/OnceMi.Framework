using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Security
{
    public class MD5
    {
        /// <summary>
        ///     获取数组的Hex值
        /// </summary>
        /// <param name="array">需要求Hex值的数组</param>
        /// <param name="uppercase">是否转大写</param>
        /// <returns>字节数组的16进制表示</returns>
        public static string ArrayToHexString(byte[] array, bool uppercase)
        {
            var hexString = "";
            var format = "x2";
            if (uppercase)
                format = "X2";
            foreach (var b in array)
                hexString += b.ToString(format);
            return hexString;
        }

        /// <summary>
        ///     对字符串进行MD5加密
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <returns>加密后的结果</returns>
        public static string MD5String(string message, bool isUpper = false, bool is16 = false)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(message));
                string md5Str = BitConverter.ToString(result);
                md5Str = md5Str.Replace("-", "");
                md5Str = isUpper ? md5Str : md5Str.ToLower();
                return is16 ? md5Str.Substring(8, 16) : md5Str;
            }
        }

        /// <summary>
        ///     对字符串进行MD5加盐加密
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <param name="salt">盐</param>
        /// <returns>加密后的结果</returns>
        public static string MD5String(string message, string salt) => MD5String(message + salt);
    }
}
