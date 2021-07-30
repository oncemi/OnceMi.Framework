using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Security
{
    /// <summary>
    ///     对称加密解密算法类
    /// </summary>
    public static class Rijndael
    {
        private static string _key;
        private static SymmetricAlgorithm _mobjCryptoService;

        /// <summary>
        ///     对称加密类的构造函数
        /// </summary>
        public static void SymmetricMethod()
        {
            _mobjCryptoService = new RijndaelManaged();
            _key = "Guz(%&hj7x89H$yuBI0456FtmaT5&fvHUFCy76*h%(HilJ$lhj!y6&(*jkP87jH7";
        }

        /// <summary>
        ///     获得密钥
        /// </summary>
        /// <returns>密钥</returns>
        private static byte[] GetLegalKey()
        {
            var sTemp = _key;
            _mobjCryptoService.GenerateKey();
            var bytTemp = _mobjCryptoService.Key;
            var keyLength = bytTemp.Length;
            if (sTemp.Length > keyLength)
                sTemp = sTemp.Substring(0, keyLength);
            else if (sTemp.Length < keyLength)
                sTemp = sTemp.PadRight(keyLength, ' ');
            return Encoding.ASCII.GetBytes(sTemp);
        }

        /// <summary>
        ///     获得初始向量IV
        /// </summary>
        /// <returns>初试向量IV</returns>
        private static byte[] GetLegalIV()
        {
            var sTemp = "E4ghj*Ghg7!rNIfb&95GUY86GfghUber57HBh(u%g6HJ($jhWk7&!hg4ui%$hjk";
            _mobjCryptoService.GenerateIV();
            var bytTemp = _mobjCryptoService.IV;
            var ivLength = bytTemp.Length;
            if (sTemp.Length > ivLength)
                sTemp = sTemp.Substring(0, ivLength);
            else if (sTemp.Length < ivLength)
                sTemp = sTemp.PadRight(ivLength, ' ');
            return Encoding.ASCII.GetBytes(sTemp);
        }

        /// <summary>
        ///     加密方法
        /// </summary>
        /// <param name="source">待加密的串</param>
        /// <returns>经过加密的串</returns>
        public static string Encrypto(string source)
        {
            var bytIn = Encoding.UTF8.GetBytes(source);
            var ms = new MemoryStream();
            _mobjCryptoService.Key = GetLegalKey();
            _mobjCryptoService.IV = GetLegalIV();
            var encrypto = _mobjCryptoService.CreateEncryptor();
            var cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();
            ms.Close();
            var bytOut = ms.ToArray();
            return Convert.ToBase64String(bytOut);
        }

        /// <summary>
        ///     解密方法
        /// </summary>
        /// <param name="source">待解密的串</param>
        /// <returns>经过解密的串</returns>
        public static string Decrypto(string source)
        {
            var bytIn = Convert.FromBase64String(source);
            var ms = new MemoryStream(bytIn, 0, bytIn.Length);
            _mobjCryptoService.Key = GetLegalKey();
            _mobjCryptoService.IV = GetLegalIV();
            var encrypto = _mobjCryptoService.CreateDecryptor();
            var cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
            var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
