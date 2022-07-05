using System;
using System.Collections.Generic;
using System.Text;

namespace OnceMi.Framework.Util.Http
{
    public class ParamConvertUtils<T> where T : class
    {
        /// <summary>
        /// 将实体类通过反射组装成字符串
        /// </summary>
        /// <param name="t">实体类</param>
        /// <returns>组装的字符串</returns>
        public static string GetEntityToString(T t)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            Type type = t.GetType();
            System.Reflection.PropertyInfo[] propertyInfos = type.GetProperties();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                sb.Append(propertyInfos[i].Name + "=" + propertyInfos[i].GetValue(t, null) + "&");
            }
            return sb.ToString();
        }

        public static T StringConvertEntity(string query, bool isfullUrl)
        {
            if (isfullUrl)
            {
                int index = query.IndexOf("?");
                if (index < 0)
                {
                    throw new Exception($"Can not find param from url '{query}'");
                }
                if (index == query.Length - 1)
                {
                    throw new Exception($"Can not find param from url '{query}'");
                }
                index = index + 1;
                query = query.Substring(index, query.Length - index);
                return StringConvertEntity(query);
            }
            else
            {
                return StringConvertEntity(query);
            }
        }

        public static T StringConvertEntity(string query)
        {
            try
            {
                string[] array = query.Split('&');
                List<string> queryParam = new List<string>();
                if (array.Length != 0)
                {
                    foreach (var item in array)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            queryParam.Add(item);
                        }
                    }
                }
                if (queryParam.Count == 0)
                {
                    return default(T);
                }

                string[] temp = null;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                foreach (string s in queryParam)
                {
                    temp = s.Split('=');
                    if (temp.Length != 2)
                    {
                        continue;
                    }
                    dictionary.Add(temp[0], HttpUtil.UrlDecode(temp[1]));
                }
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(typeof(T));
                T entry = (T)assembly.CreateInstance(typeof(T).FullName);
                StringBuilder sb = new StringBuilder();
                Type type = entry.GetType();
                System.Reflection.PropertyInfo[] propertyInfos = type.GetProperties();
                for (int i = 0; i < propertyInfos.Length; i++)
                {
                    foreach (string key in dictionary.Keys)
                    {
                        if (propertyInfos[i].Name.ToLower() == key.ToString().ToLower())
                        {
                            propertyInfos[i].SetValue(entry, GetObject(propertyInfos[i], dictionary[key]), null);
                            break;
                        }
                    }
                }
                return entry;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 转换值的类型
        /// </summary>
        /// <param name="p"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object GetObject(System.Reflection.PropertyInfo p, string value)
        {
            switch (p.PropertyType.Name.ToString().ToLower())
            {
                case "int16":
                    return Convert.ToInt16(value);
                case "int32":
                    return Convert.ToInt32(value);
                case "int64":
                    return Convert.ToInt64(value);
                case "string":
                    return Convert.ToString(value);
                case "datetime":
                    return Convert.ToDateTime(value);
                case "boolean":
                    return Convert.ToBoolean(value);
                case "char":
                    return Convert.ToChar(value);
                case "double":
                    return Convert.ToDouble(value);
                default:
                    return value;
            }
        }
    }
}
