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
    ///     RC2加密解密算法
    /// </summary>
    public static class RC2
    {
        private static ASCIIEncoding _asciiEncoding;
        private static byte[] _iv;
        private static byte[] _key;
        private static RC2CryptoServiceProvider _rc2Csp;
        private static UnicodeEncoding _textConverter;

        static RC2()
        {
            InitializeComponent();
        }

        private static void InitializeComponent()
        {
            _key = new byte[]
            {
                106,
                51,
                25,
                141,
                157,
                142,
                23,
                111,
                234,
                159,
                187,
                154,
                215,
                34,
                37,
                204
            };
            _iv = new byte[]
            {
                135,
                186,
                133,
                136,
                184,
                149,
                153,
                144
            };
            _asciiEncoding = new ASCIIEncoding();
            _textConverter = new UnicodeEncoding();
            _rc2Csp = new RC2CryptoServiceProvider();
        }

        /// <summary>
        ///     新建一个大小为10261B的文件，以便将加密数据写入固定大小的文件。
        /// </summary>
        /// <param name="filePath">文件保存的地址，包含文件名</param>
        public static string InitBinFile(string filePath)
        {
            var tmp = new byte[10261];
            try //创建文件流，将其内容全部写入0
            {
                var writeFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 512, false);

                for (var i = 0; i < 10261; i++)
                    tmp[i] = 0;
                writeFileStream.Write(tmp, 0, 10261);
                writeFileStream.Flush();
                writeFileStream.Close();
            }
            catch (IOException)
            {
                // MessageBox.Show("文件操作错误！", "错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Error,file operation error!";
            }

            return "OK";
        }

        /// <summary>
        ///     将文本数据加密后写入一个文件，其中，这个文件是用InitBinFile建立的，这个文件将被分成十块，
        ///     用来分别保存10组不同的数据，第一个byte位保留，第2位到第21位分别用来存放每块数据的长度，但
        ///     一个byte的取值为0-127，所以，用两个byte来存放一个长度。
        /// </summary>
        /// <param name="toEncryptText">要加密的文本数据</param>
        /// <param name="filePath">要写入的文件</param>
        /// <param name="dataIndex">写入第几块，取值为1--10</param>
        /// <returns>是否操作成功</returns>
        public static bool EncryptToFile(string toEncryptText, string filePath, int dataIndex)
        {
            var r = false;
            if ((dataIndex > 10) && (dataIndex < 1))
            {
                return r;
            }

            //打开要写入的文件，主要是为了保持原文件的内容不丢失
            var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);

            var index = new byte[10261];
            //将读取的内容写到byte数组
            tmpFileStream.Read(index, 0, 10261);
            tmpFileStream.Close();
            //定义基本的加密转换运算
            var Encryptor = _rc2Csp.CreateEncryptor(_key, _iv);
            var msEncrypt = new MemoryStream();
            //在此加密转换流中，加密将从csEncrypt，加密后，结果在msEncrypt流中。
            var csEncrypt = new CryptoStream(msEncrypt, Encryptor, CryptoStreamMode.Write);
            //将要加密的文本转换成UTF-16 编码，保存在tmp数组。
            var tmp = _textConverter.GetBytes(toEncryptText);
            //将tmp输入csEncrypt,将通过Encryptor来加密。
            csEncrypt.Write(tmp, 0, tmp.Length);
            //输出到msEnctypt
            csEncrypt.FlushFinalBlock();
            //将流转成byte[]
            var encrypted = msEncrypt.ToArray();
            if (encrypted.Length > 1024)
                return false;
            //得到加密后数据的大小，将结果存在指定的位置。
            index[dataIndex * 2 - 1] = Convert.ToByte(Convert.ToString(encrypted.Length / 128));
            index[dataIndex * 2] = Convert.ToByte(Convert.ToString(encrypted.Length % 128));
            //将加密后的结果写入index（覆盖）
            for (var i = 0; i < encrypted.Length; i++)
                index[1024 * (dataIndex - 1) + 21 + i] = encrypted[i];
            //建立文件流
            tmpFileStream = new FileStream(filePath, FileMode.Truncate, FileAccess.Write, FileShare.None, 1024, true);
            //写文件
            tmpFileStream.Write(index, 0, 10261);
            tmpFileStream.Flush();
            r = true;
            tmpFileStream.Close();
            return r;
        }

        /// <summary>
        ///     从一个文件中解密出一段文本，其中，这个文件是由InitBinFile建立的，并且由 EncryptToFile加密的
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <param name="dataIndex">要从哪一个块中解密</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(string filePath, int dataIndex)
        {
            var r = "";
            if ((dataIndex > 10) && (dataIndex < 1))
            {
                return r;
            }

            var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);

            var decryptor = _rc2Csp.CreateDecryptor(_key, _iv);
            var msDecrypt = new MemoryStream();
            var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write);
            var index = new byte[10261];

            tmpFileStream.Read(index, 0, 10261);
            //var startIndex = 1024 * (dataIndex - 1) + 21;
            var count = index[dataIndex * 2 - 1] * 128 + index[dataIndex * 2];
            var tmp = new byte[count];

            Array.Copy(index, 1024 * (dataIndex - 1) + 21, tmp, 0, count);
            csDecrypt.Write(tmp, 0, count);
            csDecrypt.FlushFinalBlock();
            var decrypted = msDecrypt.ToArray();
            r = _textConverter.GetString(decrypted, 0, decrypted.Length);
            tmpFileStream.Close();
            return r;
        }


        /// <summary>
        ///     将一段文本加密后保存到一个文件
        /// </summary>
        /// <param name="toEncryptText">要加密的文本数据</param>
        /// <param name="filePath">要保存的文件</param>
        /// <returns>是否加密成功</returns>
        public static bool EncryptToFile(string toEncryptText, string filePath)
        {
            bool r;
            using (var tmpFileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1024, true))
            {
                using (var encryptor = _rc2Csp.CreateEncryptor(_key, _iv))
                {
                    var msEncrypt = new MemoryStream();
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        var tmp = _textConverter.GetBytes(toEncryptText);
                        csEncrypt.Write(tmp, 0, tmp.Length);
                        csEncrypt.FlushFinalBlock();
                    }
                    var encrypted = msEncrypt.ToArray();
                    tmpFileStream.Write(encrypted, 0, encrypted.Length);
                    r = true;
                }
            }
            return r;
        }

        /// <summary>
        ///     将一个被加密的文件解密
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(string filePath)
        {
            using (var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true))
            {
                using (var decryptor = _rc2Csp.CreateDecryptor(_key, _iv))
                {
                    var msDecrypt = new MemoryStream();
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        var tmp = new byte[tmpFileStream.Length];
                        tmpFileStream.Read(tmp, 0, tmp.Length);
                        csDecrypt.Write(tmp, 0, tmp.Length);
                        csDecrypt.FlushFinalBlock();
                    }

                    var decrypted = msDecrypt.ToArray();
                    var r = _textConverter.GetString(decrypted, 0, decrypted.Length);
                    return r;
                }

            }

        }

        /// <summary>
        ///     将文本数据加密后写入一个文件，其中，这个文件是用InitBinFile建立的，这个文件将被分成十块，
        ///     用来分别保存10组不同的数据，第一个byte位保留，第2位到第21位分别用来存放每块数据的长度，但
        ///     一个byte的取值为0-127，所以，用两个byte来存放一个长度。
        /// </summary>
        /// <param name="toEncryptText">要加密的文本数据</param>
        /// <param name="filePath">要写入的文件</param>
        /// <param name="dataIndex">写入第几块，取值为1--10</param>
        /// <param name="IV">初始化向量</param>
        /// <param name="Key">加密密匙</param>
        /// <returns>是否操作成功</returns>
        public static bool EncryptToFile(string toEncryptText, string filePath, int dataIndex, byte[] IV, byte[] Key)
        {
            var r = false;
            if ((dataIndex > 10) && (dataIndex < 1))
            {
                return r;
            }
            //打开要写入的文件，主要是为了保持原文件的内容不丢失
            var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);

            var index = new byte[10261];
            //将读取的内容写到byte数组
            tmpFileStream.Read(index, 0, 10261);
            tmpFileStream.Close();
            //定义基本的加密转换运算
            using (var encryptor = _rc2Csp.CreateEncryptor(Key, IV))
            {
                var msEncrypt = new MemoryStream();
                //在此加密转换流中，加密将从csEncrypt，加密后，结果在msEncrypt流中。
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    var tmp = _textConverter.GetBytes(toEncryptText);
                    //将tmp输入csEncrypt,将通过Encryptor来加密。
                    csEncrypt.Write(tmp, 0, tmp.Length);
                    //输出到msEnctypt
                    csEncrypt.FlushFinalBlock();
                }

                //将流转成byte[]
                var encrypted = msEncrypt.ToArray();
                if (encrypted.Length > 1024)
                    return false;
                //得到加密后数据的大小，将结果存在指定的位置。
                index[dataIndex * 2 - 1] = Convert.ToByte(Convert.ToString(encrypted.Length / 128));
                index[dataIndex * 2] = Convert.ToByte(Convert.ToString(encrypted.Length % 128));
                //将加密后的结果写入index（覆盖）
                for (var i = 0; i < encrypted.Length; i++)
                    index[1024 * (dataIndex - 1) + 21 + i] = encrypted[i];
                //建立文件流
                tmpFileStream = new FileStream(filePath, FileMode.Truncate, FileAccess.Write, FileShare.None, 1024, true);
                //写文件
                tmpFileStream.Write(index, 0, 10261);
                tmpFileStream.Flush();
                r = true;
                tmpFileStream.Close();
                return r;
            }

        }

        /// <summary>
        ///     从一个文件中解密出一段文本，其中，这个文件是由InitBinFile建立的，并且由 EncryptToFile加密的
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <param name="dataIndex">要从哪一个块中解密</param>
        /// <param name="iv">初始化向量</param>
        /// <param name="key">解密密匙</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(string filePath, int dataIndex, byte[] iv, byte[] key)
        {
            var r = "";
            if ((dataIndex > 10) && (dataIndex < 1))
            {
                return r;
            }

            using (var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true))
            {
                using (var decryptor = _rc2Csp.CreateDecryptor(key, iv))
                {
                    var msDecrypt = new MemoryStream();
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        var index = new byte[10261];

                        tmpFileStream.Read(index, 0, 10261);
                        var count = index[dataIndex * 2 - 1] * 128 + index[dataIndex * 2];
                        var tmp = new byte[count];

                        Array.Copy(index, 1024 * (dataIndex - 1) + 21, tmp, 0, count);
                        csDecrypt.Write(tmp, 0, count);
                        csDecrypt.FlushFinalBlock();
                        var decrypted = msDecrypt.ToArray();
                        r = _textConverter.GetString(decrypted, 0, decrypted.Length);
                    }
                }
            }

            return r;
        }


        /// <summary>
        ///     将一段文本加密后保存到一个文件
        /// </summary>
        /// <param name="toEncryptText">要加密的文本数据</param>
        /// <param name="filePath">要保存的文件</param>
        /// <param name="iv">初始化向量</param>
        /// <param name="key">加密密匙</param>
        /// <returns>是否加密成功</returns>
        public static bool EncryptToFile(string toEncryptText, string filePath, byte[] iv, byte[] key)
        {
            using (var tmpFileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1024, true))
            {
                using (var encryptor = _rc2Csp.CreateEncryptor(key, iv))
                {
                    var msEncrypt = new MemoryStream();
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        var tmp = _textConverter.GetBytes(toEncryptText);
                        csEncrypt.Write(tmp, 0, tmp.Length);
                        csEncrypt.FlushFinalBlock();
                        var encrypted = msEncrypt.ToArray();
                        tmpFileStream.Write(encrypted, 0, encrypted.Length);
                        tmpFileStream.Flush();
                        var r = true;
                        return r;
                    }
                }
            }
        }

        /// <summary>
        ///     将一个被加密的文件解密
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <param name="iv">初始化向量</param>
        /// <param name="key">解密密匙</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(string filePath, byte[] iv, byte[] key)
        {
            using (var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true))
            {
                using (var decryptor = _rc2Csp.CreateDecryptor(key, iv))
                {
                    var msDecrypt = new MemoryStream();
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        var tmp = new byte[tmpFileStream.Length];
                        tmpFileStream.Read(tmp, 0, tmp.Length);
                        csDecrypt.Write(tmp, 0, tmp.Length);
                        csDecrypt.FlushFinalBlock();
                        var decrypted = msDecrypt.ToArray();
                        var r = _textConverter.GetString(decrypted, 0, decrypted.Length);
                        return r;
                    }
                }
            }
        }

        /// <summary>
        ///     设置加密或解密的初始化向量
        /// </summary>
        /// <param name="s">长度等于8的ASCII字符集的字符串</param>
        public static void SetIV(string s)
        {
            if (s.Length != 8)
            {
                // MessageBox.Show("输入的字符串必须为长度为8的且属于ASCII字符集的字符串");
                _iv = null;
                return;
            }

            try
            {
                _iv = _asciiEncoding.GetBytes(s);
            }
            catch (Exception)
            {
                // MessageBox.Show("输入的字符串必须为长度为8的且属于ASCII字符集的字符串");
                _iv = null;
            }
        }


        /// <summary>
        ///     设置加密或解密的密匙
        /// </summary>
        /// <param name="s">长度等于16的ASCII字符集的字符串</param>
        public static void SetKey(string s)
        {
            if (s.Length != 16)
            {
                // MessageBox.Show("输入的字符串必须为长度为16的且属于ASCII字符集的字符串");
                _key = null;
                return;
            }

            try
            {
                _key = _asciiEncoding.GetBytes(s);
            }
            catch (Exception)
            {
                //MessageBox.Show("输入的字符串必须为长度为16的且属于ASCII字符集的字符串");
                _key = null;
            }
        }


    }
}
