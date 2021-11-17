using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Security
{
    public class SHA
    {

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果(返回长度为44字节的字符串)</returns>
        public static string SHA256(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            using (SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("X2"));
                }
                return builder.ToString().ToLower();
            }
        }

        /// <summary>  
        /// SHA1 加密，返回小写字符串
        /// </summary>  
        /// <param name="content">需要加密字符串</param>  
        /// <param name="encode">指定加密编码</param>  
        /// <returns>返回40位小写写字符串</returns>  
        public static string SHA1(string content)
        {
            using (SHA1 sha1 = System.Security.Cryptography.SHA1.Create())
            {
                byte[] bytes_in = Encoding.Default.GetBytes(content);
                byte[] bytes_out = sha1.ComputeHash(bytes_in);
                string result = BitConverter.ToString(bytes_out);
                result = result.Replace("-", "");
                return result.ToLower();
            }
        }

        /// <summary>
        /// HmacSHA256加密
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string HmacSHA256(string message, string secret)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentNullException(nameof(secret));
            }

            byte[] keyByte = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        /// <summary>
        /// HmacSHA1加密
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string HmacSHA1(string message, string secret)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentNullException(nameof(secret));
            }

            byte[] keyByte = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha1 = new HMACSHA1(keyByte))
            {
                byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
