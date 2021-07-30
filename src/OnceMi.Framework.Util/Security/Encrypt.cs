using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Security
{
    /// <summary>
    /// 加解密类
    /// </summary>
    public class Encrypt
    {
        public enum MD5EncryptLength
        {
            M16,
            M32
        }

        /// <summary>
        /// 加密密钥，需要在config配置文件中AppSettings节点中配置desSecret值，若未配置，默认取“masuit”的MD5值
        /// </summary>
        public static string DefaultEncryptKey
        {
            get
            {
                return MD5String("masuit_lalala_geeiot");
            }
        }

        #region HmacSHA

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

        #endregion

        #region DES

        /// <summary>
        /// 使用默认加密
        /// </summary>
        /// <param name="strText">被加密的字符串</param>
        /// <returns>加密后的数据</returns>
        public static string DesEncrypt(string strText)
        {
            try
            {
                return DesEncrypt(strText, DefaultEncryptKey);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 使用默认解密
        /// </summary>
        /// <param name="strText">需要解密的 字符串</param>
        /// <returns>解密后的数据</returns>
        public static string DesDecrypt(string strText)
        {
            try
            {
                return DesDecrypt(strText, DefaultEncryptKey);
            }
            catch
            {
                return "";
            }
        }

        /// <summary> 
        /// 解密字符串
        /// 加密密钥必须为8位
        /// </summary> 
        /// <param name="strText">被解密的字符串</param> 
        /// <param name="strEncrKey">密钥</param> 
        /// <returns>解密后的数据</returns> 
        public static string DesEncrypt(string strText, string strEncrKey)
        {
            if (strEncrKey.Length < 8)
            {
                throw new Exception("密钥长度无效，密钥必须是8位！");
            }

            StringBuilder ret = new StringBuilder();
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(strText);
            des.Key = Encoding.ASCII.GetBytes(strEncrKey.Substring(0, 8));
            des.IV = Encoding.ASCII.GetBytes(strEncrKey.Substring(0, 8));
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat($"{b:X2}");
            }

            return ret.ToString();
        }

        /// <summary>
        /// DES加密文件
        /// </summary>
        /// <param name="fin">文件输入流</param>
        /// <param name="outFilePath">文件输出路径</param>
        /// <param name="strEncrKey">加密密钥</param>
        public static void DesEncrypt(FileStream fin, string outFilePath, string strEncrKey)
        {
            byte[] iv =
            {
                0x12,
                0x34,
                0x56,
                0x78,
                0x90,
                0xAB,
                0xCD,
                0xEF
            };
            var byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
            using (fin)
            {
                using (FileStream fout = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fout.SetLength(0);
                    byte[] bin = new byte[100];
                    long rdlen = 0;
                    long totlen = fin.Length;
                    DES des = new DESCryptoServiceProvider();
                    var encStream = new CryptoStream(fout, des.CreateEncryptor(byKey, iv), CryptoStreamMode.Write);
                    while (rdlen < totlen)
                    {
                        var len = fin.Read(bin, 0, 100);
                        encStream.Write(bin, 0, len);
                        rdlen += len;
                    }
                }
            }
        }

        /// <summary>
        /// DES解密文件
        /// </summary>
        /// <param name="fin">输入文件流</param>
        /// <param name="outFilePath">文件输出路径</param>
        /// <param name="sDecrKey">解密密钥</param>
        public static void DesDecrypt(FileStream fin, string outFilePath, string sDecrKey)
        {
            byte[] iv =
            {
                0x12,
                0x34,
                0x56,
                0x78,
                0x90,
                0xAB,
                0xCD,
                0xEF
            };
            var byKey = Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8));
            using (fin)
            {
                using (FileStream fout = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fout.SetLength(0);
                    byte[] bin = new byte[100];
                    long rdlen = 0;
                    long totlen = fin.Length;
                    DES des = new DESCryptoServiceProvider();
                    CryptoStream encStream = new CryptoStream(fout, des.CreateDecryptor(byKey, iv), CryptoStreamMode.Write);
                    while (rdlen < totlen)
                    {
                        var len = fin.Read(bin, 0, 100);
                        encStream.Write(bin, 0, len);
                        rdlen += len;
                    }
                }
            }
        }

        /// <summary>
        ///     DES解密算法
        ///     密钥为8位
        /// </summary>
        /// <param name="pToDecrypt">需要解密的字符串</param>
        /// <param name="sKey">密钥</param>
        /// <returns>解密后的数据</returns>
        public static string DesDecrypt(string pToDecrypt, string sKey)
        {
            if (sKey.Length < 8)
            {
                throw new Exception("密钥长度无效，密钥必须是8位！");
            }

            var ms = new MemoryStream();

            var des = new DESCryptoServiceProvider();
            var inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.ASCII.GetBytes(sKey.Substring(0, 8));
            des.IV = Encoding.ASCII.GetBytes(sKey.Substring(0, 8));
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion

        #region Base64

        /// <summary>
        /// Base64加密，采用utf8编码方式加密
        /// </summary>
        /// <param name="source">待加密的明文</param>
        /// <returns>加密后的字符串</returns>
        public static string Base64Encode(string source)
        {
            return Base64Encode(Encoding.UTF8, source);
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="encodeType">加密采用的编码方式</param>
        /// <param name="source">待加密的明文</param>
        /// <returns></returns>
        public static string Base64Encode(Encoding encodeType, string source)
        {
            string encode = string.Empty;
            byte[] bytes = encodeType.GetBytes(source);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = source;
            }
            return encode;
        }

        /// <summary>
        /// Base64解密，采用utf8编码方式解密
        /// </summary>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        public static string Base64Decode(string result)
        {
            return Base64Decode(Encoding.UTF8, result);
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="encodeType">解密采用的编码方式，注意和加密时采用的方式一致</param>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        public static string Base64Decode(Encoding encodeType, string result)
        {
            string decode = string.Empty;
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encodeType.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }

        #endregion

        #region AES

        /// <summary>  
        /// AES encrypt
        /// </summary>  
        /// <param name="data">Raw data</param>  
        /// <param name="key">Key, requires 32 bits</param>  
        /// <param name="vector">IV,requires 16 bits</param>  
        /// <returns>Encrypted string</returns>  
        public static string AESEncrypt(string data, string key, string vector)
        {
            Verify.IsNotEmpty(data, nameof(data));

            Verify.IsNotEmpty(key, nameof(key));
            Verify.IsNotOutOfRange(key.Length, 16, 256, nameof(key));

            Verify.IsNotEmpty(vector, nameof(vector));
            Verify.IsNotOutOfRange(vector.Length, 16, 256, nameof(vector));

            byte[] plainBytes = Encoding.UTF8.GetBytes(data);

            var encryptBytes = AESEncrypt(plainBytes, key, vector);
            if (encryptBytes == null)
            {
                return null;
            }
            return Convert.ToBase64String(encryptBytes);
        }

        /// <summary>
        /// AES encrypt
        /// </summary>
        /// <param name="data">Raw data</param>  
        /// <param name="key">Key, requires 16 bits</param>  
        /// <param name="vector">IV,requires 16 bits</param>  
        /// <returns>Encrypted byte array</returns>  
        public static byte[] AESEncrypt(byte[] data, string key, string vector)
        {
            Verify.IsNotEmpty(data, nameof(data));

            Verify.IsNotEmpty(key, nameof(key));
            Verify.IsNotOutOfRange(key.Length, 16, 256, nameof(key));

            Verify.IsNotEmpty(vector, nameof(vector));
            Verify.IsNotOutOfRange(vector.Length, 16, 256, nameof(vector));

            byte[] bKey = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length, (char)0x00)), bKey, bKey.Length);

            byte[] bVector = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(vector.PadRight(bVector.Length, (char)0x00)), bVector, bVector.Length);

            return AESEncrypt(data, bKey, bVector);
        }

        /// <summary>
        /// AES encrypt
        /// </summary>
        /// <param name="data">Raw data</param>
        /// <param name="key">Key, requires 16 bits</param>
        /// <param name="vector">IV,requires 16 bits</param>
        /// <returns></returns>
        public static byte[] AESEncrypt(byte[] data, byte[] key, byte[] vector)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (key == null || key.Length != 16)
            {
                throw new ArgumentException(nameof(key));
            }
            if (vector == null || vector.Length != 16)
            {
                throw new ArgumentException(nameof(vector));
            }

            byte[] encryptData = null;
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                try
                {
                    using (MemoryStream Memory = new MemoryStream())
                    {
                        using (CryptoStream Encryptor = new CryptoStream(Memory, aes.CreateEncryptor(key, vector), CryptoStreamMode.Write))
                        {
                            Encryptor.Write(data, 0, data.Length);
                            Encryptor.FlushFinalBlock();

                            encryptData = Memory.ToArray();
                        }
                    }
                }
                catch
                {
                    encryptData = null;
                }
                return encryptData;
            }
        }

        /// <summary>  
        ///  AES decrypt
        /// </summary>  
        /// <param name="data">Encrypted data</param>  
        /// <param name="key">Key, requires 32 bits</param>  
        /// <param name="vector">IV,requires 16 bits</param>  
        /// <returns>Decrypted string</returns>  
        public static string AESDecrypt(string data, string key, string vector)
        {
            Verify.IsNotEmpty(data, nameof(data));

            Verify.IsNotEmpty(key, nameof(key));
            Verify.IsNotOutOfRange(key.Length, 16, 256, nameof(key));

            Verify.IsNotEmpty(vector, nameof(vector));
            Verify.IsNotOutOfRange(vector.Length, 16, 256, nameof(vector));

            byte[] encryptedBytes = Convert.FromBase64String(data);
            byte[] decryptBytes = AESDecrypt(encryptedBytes, key, vector);

            if (decryptBytes == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(decryptBytes);
        }

        /// <summary>  
        ///  AES decrypt
        /// </summary>  
        /// <param name="data">Encrypted data</param>  
        /// <param name="key">Key, requires 16 bits</param>  
        /// <param name="vector">IV,requires 16 bits</param>  
        /// <returns>Decrypted byte array</returns>  

        public static byte[] AESDecrypt(byte[] data, string key, string vector)
        {
            Verify.IsNotEmpty(data, nameof(data));

            Verify.IsNotEmpty(key, nameof(key));
            Verify.IsNotOutOfRange(key.Length, 16, 256, nameof(key));

            Verify.IsNotEmpty(vector, nameof(vector));
            Verify.IsNotOutOfRange(vector.Length, 16, 256, nameof(vector));

            byte[] bKey = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length, (char)0x00)), bKey, bKey.Length);

            byte[] bVector = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(vector.PadRight(bVector.Length, (char)0x00)), bVector, bVector.Length);

            return AESDecrypt(data, bKey, bVector);
        }

        /// <summary>
        /// AES decrypt
        /// </summary>
        /// <param name="data">Encrypted data</param>
        /// <param name="key">Key, requires 16 bits</param>
        /// <param name="vector">IV,requires 16 bits</param>
        /// <returns></returns>
        public static byte[] AESDecrypt(byte[] data, byte[] key, byte[] vector)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (key == null || key.Length != 16)
            {
                throw new ArgumentException(nameof(key));
            }
            if (vector == null || vector.Length != 16)
            {
                throw new ArgumentException(nameof(vector));
            }

            byte[] decryptedData = null; // decrypted data
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                try
                {
                    using (MemoryStream Memory = new MemoryStream(data))
                    {
                        using (CryptoStream Decryptor = new CryptoStream(Memory, aes.CreateDecryptor(key, vector), CryptoStreamMode.Read))
                        {
                            using (MemoryStream tempMemory = new MemoryStream())
                            {
                                byte[] Buffer = new byte[1024];
                                Int32 readBytes = 0;
                                while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
                                {
                                    tempMemory.Write(Buffer, 0, readBytes);
                                }

                                decryptedData = tempMemory.ToArray();
                            }
                        }
                    }
                }
                catch
                {
                    decryptedData = null;
                }
                return decryptedData;
            }
        }

        #endregion

        #region SHA

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果(返回长度为44字节的字符串)</returns>
        public static string SHA256(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            byte[] hash = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("X2"));
            }
            return builder.ToString().ToLower();
        }

        /// <summary>  
        /// SHA1 加密，返回小写字符串
        /// </summary>  
        /// <param name="content">需要加密字符串</param>  
        /// <param name="encode">指定加密编码</param>  
        /// <returns>返回40位小写写字符串</returns>  
        public static string SHA1(string content)
        {
            try
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] bytes_in = Encoding.Default.GetBytes(content);
                byte[] bytes_out = sha1.ComputeHash(bytes_in);
                sha1.Dispose();
                string result = BitConverter.ToString(bytes_out);
                result = result.Replace("-", "");
                return result.ToLower();
            }
            catch (Exception ex)
            {
                throw new Exception("SHA1加密出错：" + ex.Message);
            }
        }

        #endregion

        #region MD5

        /// <summary> 
        /// MD5加密
        /// </summary> 
        /// <param name="strText">原数据</param> 
        /// <returns>MD5字符串</returns> 
        public static string MD5Encrypt(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.Default.GetBytes(strText));
            return Encoding.Default.GetString(result);
        }

        /// <summary>
        ///     MD5加密
        /// </summary>
        /// <param name="pToEncrypt">加密字符串</param>
        /// <param name="sKey">密钥Key</param>
        /// <returns>加密后的字符串</returns>
        public static string MD5Encrypt(string pToEncrypt, string sKey)
        {
            var des = new DESCryptoServiceProvider();
            var inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
            des.Key = Encoding.ASCII.GetBytes(sKey);
            des.IV = Encoding.ASCII.GetBytes(sKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            var ret = new StringBuilder();
            foreach (var b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }

            ret.ToString();
            return ret.ToString();
        }

        /// <summary>
        ///     MD5解密
        /// </summary>
        /// <param name="pToDecrypt">解密字符串</param>
        /// <param name="sKey">密钥Key</param>
        /// <returns>解密后的数据</returns>
        public static string MD5Decrypt(string pToDecrypt, string sKey)
        {
            var des = new DESCryptoServiceProvider();

            var inputByteArray = new byte[pToDecrypt.Length / 2];
            for (var x = 0; x < pToDecrypt.Length / 2; x++)
            {
                var i = Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.ASCII.GetBytes(sKey);
            des.IV = Encoding.ASCII.GetBytes(sKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        //number of bits to rotate in tranforming
        private const int S11 = 7;

        private const int S12 = 12;
        private const int S13 = 17;
        private const int S14 = 22;
        private const int S21 = 5;
        private const int S22 = 9;
        private const int S23 = 14;
        private const int S24 = 20;
        private const int S31 = 4;
        private const int S32 = 11;
        private const int S33 = 16;
        private const int S34 = 23;
        private const int S41 = 6;
        private const int S42 = 10;
        private const int S43 = 15;
        private const int S44 = 21;

        //static state variables
        private static uint A;

        private static uint B;
        private static uint C;
        private static uint D;

        private static uint F(uint x, uint y, uint z)
        {
            return (x & y) | (~x & z);
        }

        private static uint G(uint x, uint y, uint z)
        {
            return (x & z) | (y & ~z);
        }

        private static uint H(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }

        private static uint I(uint x, uint y, uint z)
        {
            return y ^ (x | ~z);
        }

        private static void FF(ref uint a, uint b, uint c, uint d, uint mj, int s, uint ti)
        {
            a = a + F(b, c, d) + mj + ti;
            a = (a << s) | (a >> (32 - s));
            a += b;
        }

        private static void GG(ref uint a, uint b, uint c, uint d, uint mj, int s, uint ti)
        {
            a = a + G(b, c, d) + mj + ti;
            a = (a << s) | (a >> (32 - s));
            a += b;
        }

        private static void HH(ref uint a, uint b, uint c, uint d, uint mj, int s, uint ti)
        {
            a = a + H(b, c, d) + mj + ti;
            a = (a << s) | (a >> (32 - s));
            a += b;
        }

        private static void II(ref uint a, uint b, uint c, uint d, uint mj, int s, uint ti)
        {
            a = a + I(b, c, d) + mj + ti;
            a = (a << s) | (a >> (32 - s));
            a += b;
        }

        private static void MD5_Init()
        {
            A = 0x67452301; //in memory, is 0x01234567
            B = 0xefcdab89; //in memory, is 0x89abcdef
            C = 0x98badcfe; //in memory, is 0xfedcba98
            D = 0x10325476; //in memory, is 0x76543210
        }

        private static uint[] MD5_Append(byte[] input)
        {
            int zeros;
            var ones = 1;
            int size;
            var n = input.Length;
            var m = n % 64;
            if (m < 56)
            {
                zeros = 55 - m;
                size = n - m + 64;
            }
            else if (m == 56)
            {
                zeros = 0;
                ones = 0;
                size = n + 8;
            }
            else
            {
                zeros = 63 - m + 56;
                size = n + 64 - m + 64;
            }

            var bs = new ArrayList(input);
            if (ones == 1)
                bs.Add((byte)0x80); // 0x80 = $10000000
            for (var i = 0; i < zeros; i++)
                bs.Add((byte)0);

            var N = (ulong)n * 8;
            var h1 = (byte)(N & 0xFF);
            var h2 = (byte)((N >> 8) & 0xFF);
            var h3 = (byte)((N >> 16) & 0xFF);
            var h4 = (byte)((N >> 24) & 0xFF);
            var h5 = (byte)((N >> 32) & 0xFF);
            var h6 = (byte)((N >> 40) & 0xFF);
            var h7 = (byte)((N >> 48) & 0xFF);
            var h8 = (byte)(N >> 56);
            bs.Add(h1);
            bs.Add(h2);
            bs.Add(h3);
            bs.Add(h4);
            bs.Add(h5);
            bs.Add(h6);
            bs.Add(h7);
            bs.Add(h8);
            var ts = (byte[])bs.ToArray(typeof(byte));

            /* Decodes input (byte[]) into output (UInt32[]). Assumes len is
            * a multiple of 4.
           */
            var output = new uint[size / 4];
            for (long i = 0, j = 0; i < size; j++, i += 4)
                output[j] = (uint)(ts[i] | (ts[i + 1] << 8) | (ts[i + 2] << 16) | (ts[i + 3] << 24));
            return output;
        }

        private static uint[] MD5_Trasform(uint[] x)
        {
            uint a, b, c, d;

            for (var k = 0; k < x.Length; k += 16)
            {
                a = A;
                b = B;
                c = C;
                d = D;

                /* Round 1 */
                FF(ref a, b, c, d, x[k + 0], S11, 0xd76aa478); /* 1 */
                FF(ref d, a, b, c, x[k + 1], S12, 0xe8c7b756); /* 2 */
                FF(ref c, d, a, b, x[k + 2], S13, 0x242070db); /* 3 */
                FF(ref b, c, d, a, x[k + 3], S14, 0xc1bdceee); /* 4 */
                FF(ref a, b, c, d, x[k + 4], S11, 0xf57c0faf); /* 5 */
                FF(ref d, a, b, c, x[k + 5], S12, 0x4787c62a); /* 6 */
                FF(ref c, d, a, b, x[k + 6], S13, 0xa8304613); /* 7 */
                FF(ref b, c, d, a, x[k + 7], S14, 0xfd469501); /* 8 */
                FF(ref a, b, c, d, x[k + 8], S11, 0x698098d8); /* 9 */
                FF(ref d, a, b, c, x[k + 9], S12, 0x8b44f7af); /* 10 */
                FF(ref c, d, a, b, x[k + 10], S13, 0xffff5bb1); /* 11 */
                FF(ref b, c, d, a, x[k + 11], S14, 0x895cd7be); /* 12 */
                FF(ref a, b, c, d, x[k + 12], S11, 0x6b901122); /* 13 */
                FF(ref d, a, b, c, x[k + 13], S12, 0xfd987193); /* 14 */
                FF(ref c, d, a, b, x[k + 14], S13, 0xa679438e); /* 15 */
                FF(ref b, c, d, a, x[k + 15], S14, 0x49b40821); /* 16 */

                /* Round 2 */
                GG(ref a, b, c, d, x[k + 1], S21, 0xf61e2562); /* 17 */
                GG(ref d, a, b, c, x[k + 6], S22, 0xc040b340); /* 18 */
                GG(ref c, d, a, b, x[k + 11], S23, 0x265e5a51); /* 19 */
                GG(ref b, c, d, a, x[k + 0], S24, 0xe9b6c7aa); /* 20 */
                GG(ref a, b, c, d, x[k + 5], S21, 0xd62f105d); /* 21 */
                GG(ref d, a, b, c, x[k + 10], S22, 0x2441453); /* 22 */
                GG(ref c, d, a, b, x[k + 15], S23, 0xd8a1e681); /* 23 */
                GG(ref b, c, d, a, x[k + 4], S24, 0xe7d3fbc8); /* 24 */
                GG(ref a, b, c, d, x[k + 9], S21, 0x21e1cde6); /* 25 */
                GG(ref d, a, b, c, x[k + 14], S22, 0xc33707d6); /* 26 */
                GG(ref c, d, a, b, x[k + 3], S23, 0xf4d50d87); /* 27 */
                GG(ref b, c, d, a, x[k + 8], S24, 0x455a14ed); /* 28 */
                GG(ref a, b, c, d, x[k + 13], S21, 0xa9e3e905); /* 29 */
                GG(ref d, a, b, c, x[k + 2], S22, 0xfcefa3f8); /* 30 */
                GG(ref c, d, a, b, x[k + 7], S23, 0x676f02d9); /* 31 */
                GG(ref b, c, d, a, x[k + 12], S24, 0x8d2a4c8a); /* 32 */

                /* Round 3 */
                HH(ref a, b, c, d, x[k + 5], S31, 0xfffa3942); /* 33 */
                HH(ref d, a, b, c, x[k + 8], S32, 0x8771f681); /* 34 */
                HH(ref c, d, a, b, x[k + 11], S33, 0x6d9d6122); /* 35 */
                HH(ref b, c, d, a, x[k + 14], S34, 0xfde5380c); /* 36 */
                HH(ref a, b, c, d, x[k + 1], S31, 0xa4beea44); /* 37 */
                HH(ref d, a, b, c, x[k + 4], S32, 0x4bdecfa9); /* 38 */
                HH(ref c, d, a, b, x[k + 7], S33, 0xf6bb4b60); /* 39 */
                HH(ref b, c, d, a, x[k + 10], S34, 0xbebfbc70); /* 40 */
                HH(ref a, b, c, d, x[k + 13], S31, 0x289b7ec6); /* 41 */
                HH(ref d, a, b, c, x[k + 0], S32, 0xeaa127fa); /* 42 */
                HH(ref c, d, a, b, x[k + 3], S33, 0xd4ef3085); /* 43 */
                HH(ref b, c, d, a, x[k + 6], S34, 0x4881d05); /* 44 */
                HH(ref a, b, c, d, x[k + 9], S31, 0xd9d4d039); /* 45 */
                HH(ref d, a, b, c, x[k + 12], S32, 0xe6db99e5); /* 46 */
                HH(ref c, d, a, b, x[k + 15], S33, 0x1fa27cf8); /* 47 */
                HH(ref b, c, d, a, x[k + 2], S34, 0xc4ac5665); /* 48 */

                /* Round 4 */
                II(ref a, b, c, d, x[k + 0], S41, 0xf4292244); /* 49 */
                II(ref d, a, b, c, x[k + 7], S42, 0x432aff97); /* 50 */
                II(ref c, d, a, b, x[k + 14], S43, 0xab9423a7); /* 51 */
                II(ref b, c, d, a, x[k + 5], S44, 0xfc93a039); /* 52 */
                II(ref a, b, c, d, x[k + 12], S41, 0x655b59c3); /* 53 */
                II(ref d, a, b, c, x[k + 3], S42, 0x8f0ccc92); /* 54 */
                II(ref c, d, a, b, x[k + 10], S43, 0xffeff47d); /* 55 */
                II(ref b, c, d, a, x[k + 1], S44, 0x85845dd1); /* 56 */
                II(ref a, b, c, d, x[k + 8], S41, 0x6fa87e4f); /* 57 */
                II(ref d, a, b, c, x[k + 15], S42, 0xfe2ce6e0); /* 58 */
                II(ref c, d, a, b, x[k + 6], S43, 0xa3014314); /* 59 */
                II(ref b, c, d, a, x[k + 13], S44, 0x4e0811a1); /* 60 */
                II(ref a, b, c, d, x[k + 4], S41, 0xf7537e82); /* 61 */
                II(ref d, a, b, c, x[k + 11], S42, 0xbd3af235); /* 62 */
                II(ref c, d, a, b, x[k + 2], S43, 0x2ad7d2bb); /* 63 */
                II(ref b, c, d, a, x[k + 9], S44, 0xeb86d391); /* 64 */

                A += a;
                B += b;
                C += c;
                D += d;
            }

            return new[]
            {
                A,
                B,
                C,
                D
            };
        }

        /// <summary>
        ///     MD5对数组数据加密
        /// </summary>
        /// <param name="input">包含需要加密的数据的数组</param>
        /// <returns>加密后的字节流</returns>
        public static byte[] MD5Array(byte[] input)
        {
            MD5_Init();
            var block = MD5_Append(input);
            var bits = MD5_Trasform(block);

            var output = new byte[bits.Length * 4];
            for (int i = 0, j = 0; i < bits.Length; i++, j += 4)
            {
                output[j] = (byte)(bits[i] & 0xff);
                output[j + 1] = (byte)((bits[i] >> 8) & 0xff);
                output[j + 2] = (byte)((bits[i] >> 16) & 0xff);
                output[j + 3] = (byte)((bits[i] >> 24) & 0xff);
            }

            return output;
        }

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
        public static string MD5String(string message, Encrypt.MD5EncryptLength encryptLength = Encrypt.MD5EncryptLength.M32)
        {
            var c = message.ToCharArray();
            var b = new byte[c.Length];
            for (var i = 0; i < c.Length; i++)
                b[i] = (byte)c[i];
            var digest = MD5Array(b);
            if (encryptLength == Encrypt.MD5EncryptLength.M32)
            {
                return ArrayToHexString(digest, false);
            }
            else
            {
                return ArrayToHexString(digest, false).Substring(8, 16);
            }
        }

        /// <summary>
        ///     对字符串进行MD5加盐加密
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <param name="salt">盐</param>
        /// <returns>加密后的结果</returns>
        public static string MD5String(string message, string salt) => MD5String(message + salt);

        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="fileName">需要求MD5值的文件的文件名及路径</param>
        /// <returns>MD5字符串</returns>
        public static string MD5File(string fileName)
        {
            var fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
            var array = new byte[fs.Length];
            fs.Read(array, 0, (int)fs.Length);
            var digest = MD5Array(array);
            fs.Close();
            return ArrayToHexString(digest, false);
        }

        /// <summary>
        ///     测试MD5加密算法的函数
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <returns>加密后的 数据</returns>
        private static string MD5Test(string message)
        {
            return "rnMD5 (" + "message" + ") = " + MD5String(message);
        }

        /// <summary>
        ///     MD5加密算法测试用数据
        /// </summary>
        /// <returns> </returns>
        private static string TestSuite()
        {
            var s = "";
            s += MD5Test("");
            s += MD5Test("a");
            s += MD5Test("abc");
            s += MD5Test("message digest");
            s += MD5Test("abcdefghijklmnopqrstuvwxyz");
            s += MD5Test("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
            s += MD5Test("12345678901234567890123456789012345678901234567890123456789012345678901234567890");
            return s;
        }

        #endregion
    }

    /// <summary>
    /// 验证
    /// </summary>
    class Verify
    {
        internal Verify()
        {
        }

        public static void IsNotEmpty(Guid argument, string argumentName)
        {
            if (argument == Guid.Empty)
            {
                throw new ArgumentException(string.Format("\"{0}\" 不能为空Guid.", argumentName), argumentName);
            }
        }

        public static void IsNotEmpty(string argument, string argumentName)
        {
            if (string.IsNullOrEmpty((argument ?? string.Empty).Trim()))
            {
                throw new ArgumentException(string.Format("\"{0}\" 不能为空.", argumentName), argumentName);
            }
        }

        public static void IsNotOutOfLength(string argument, int length, string argumentName)
        {
            if (argument.Trim().Length > length)
            {
                throw new ArgumentException(string.Format("\"{0}\" 不能超过 {1} 字符.", argumentName, length), argumentName);
            }
        }

        public static void IsNotNull(object argument, string argumentName, string message = "")
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName, message);
            }
        }

        public static void IsNotNegative(int argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegativeOrZero(int argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegative(long argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }
        public static void IsNotNegativeOrZero(long argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegative(float argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegativeOrZero(float argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }
        public static void IsNotNegative(decimal argument, string argumentName)
        {
            if (argument < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegativeOrZero(decimal argument, string argumentName)
        {
            if (argument <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotInvalidDate(DateTime argument, string argumentName)
        {
            DateTime MinDate = new DateTime(1900, 1, 1);
            DateTime MaxDate = new DateTime(9999, 12, 31, 23, 59, 59, 999);

            if (!((argument >= MinDate) && (argument <= MaxDate)))
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotInPast(DateTime argument, string argumentName)
        {
            if (argument < DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotInFuture(DateTime argument, string argumentName)
        {
            if (argument > DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegative(TimeSpan argument, string argumentName)
        {
            if (argument < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotNegativeOrZero(TimeSpan argument, string argumentName)
        {
            if (argument <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void IsNotEmpty<T>(ICollection<T> argument, string argumentName)
        {
            IsNotNull(argument, argumentName, "集合不能为Null");

            if (argument.Count == 0)
            {
                throw new ArgumentException("集合不能为空.", argumentName);
            }
        }
        public static void IsNotOutOfRange(int argument, int min, int max, string argumentName)
        {
            if ((argument < min) || (argument > max))
            {
                throw new ArgumentOutOfRangeException(argumentName, string.Format("{0} 必须在此区间 \"{1}\"-\"{2}\".", argumentName, min, max));
            }
        }

        public static void IsNotExistsFile(string argument, string argumentName)
        {
            IsNotEmpty(argument, argumentName);

            if (!File.Exists(argument))
            {
                throw new ArgumentException(string.Format("\"{0}\" 文件不存在", argumentName), argumentName);
            }
        }

        public static void IsNotExistsDirectory(string argument, string argumentName)
        {
            IsNotEmpty(argument, argumentName);

            if (!Directory.Exists(argument))
            {
                throw new ArgumentException(string.Format("\"{0}\" 目录不存在", argumentName), argumentName);
            }
        }
    }
}

