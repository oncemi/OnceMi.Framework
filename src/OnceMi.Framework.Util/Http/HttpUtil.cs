using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OnceMi.Framework.Util.Http
{
    /// <summary>
    /// Http连接操作帮助类
    /// </summary>
    public class HttpUtil
    {
        public static string UrlEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return Uri.EscapeDataString(str);
        }

        public static string UrlDecode(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return Uri.UnescapeDataString(str);
        }

        public static string UrlDecode(string s, Encoding e)
        {
            if (null == s)
                return null;
            if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
                return s;
            if (e == null)
                e = Encoding.UTF8;

            long len = s.Length;
            var bytes = new List<byte>();
            int xchar;
            char ch;

            for (int i = 0; i < len; i++)
            {
                ch = s[i];
                if (ch == '%' && i + 2 < len && s[i + 1] != '%')
                {
                    if (s[i + 1] == 'u' && i + 5 < len)
                    {
                        // unicode hex sequence
                        xchar = GetChar(s, i + 2, 4);
                        if (xchar != -1)
                        {
                            WriteCharBytes(bytes, (char)xchar, e);
                            i += 5;
                        }
                        else
                            WriteCharBytes(bytes, '%', e);
                    }
                    else if ((xchar = GetChar(s, i + 1, 2)) != -1)
                    {
                        WriteCharBytes(bytes, (char)xchar, e);
                        i += 2;
                    }
                    else
                    {
                        WriteCharBytes(bytes, '%', e);
                    }
                    continue;
                }
                if (ch == '+')
                    WriteCharBytes(bytes, ' ', e);
                else
                    WriteCharBytes(bytes, ch, e);
            }
            byte[] buf = bytes.ToArray();
            bytes = null;
            return e.GetString(buf);
        }

        #region Decode private


        static void WriteCharBytes(IList buf, char ch, Encoding e)
        {
            if (ch > 255)
            {
                foreach (byte b in e.GetBytes(new char[] { ch }))
                    buf.Add(b);
            }
            else
                buf.Add((byte)ch);
        }

        static int GetInt(byte b)
        {
            char c = (char)b;
            if (c >= '0' && c <= '9')
                return c - '0';

            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;

            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;

            return -1;
        }

        static int GetChar(string str, int offset, int length)
        {
            int val = 0;
            int end = length + offset;
            for (int i = offset; i < end; i++)
            {
                char c = str[i];
                if (c > 127)
                    return -1;

                int current = GetInt((byte)c);
                if (current == -1)
                    return -1;
                val = (val << 4) + current;
            }

            return val;
        }

        #endregion

        /// <summary>
        /// 转换cookie字符串到CookieCollection
        /// </summary>
        /// <param name="ck"></param>
        /// <returns></returns>
        public static CookieCollection ConvertCookieString(string ck)
        {
            CookieCollection cc = new CookieCollection();
            string[] cookiesarray = ck.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < cookiesarray.Length; i++)
            {
                string[] cookiesarray_2 = cookiesarray[i].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < cookiesarray_2.Length; j++)
                {
                    string[] cookiesarray_3 = cookiesarray_2[j].Trim().Split("=".ToCharArray());
                    if (cookiesarray_3.Length == 2)
                    {
                        string cname = cookiesarray_3[0].Trim();
                        string cvalue = cookiesarray_3[1].Trim();
                        if (cname.ToLower() != "domain" && cname.ToLower() != "path" && cname.ToLower() != "expires")
                        {
                            Cookie c = new Cookie(cname, cvalue);
                            cc.Add(c);
                        }
                    }
                }
            }
            return cc;
        }
    }
}