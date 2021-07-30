using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace OnceMi.AspNetCore.MQ.Utils
{
    class JsonUtil
    {
        private static JsonSerializerOptions BuildJsonSerializerOptions(JsonSerializerOptions options)
        {
            if (options != null)
            {
                if (options.Encoder == null)
                    options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            }
            else
            {
                options = new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                };
            }
            //DateTimeConverter
            if (options.Converters.Count == 0)
            {
                options.Converters.Add(new DateTimeConverter());
                options.Converters.Add(new DateTimeNullableConverter());
            }
            else
            {
                IList<JsonConverter> converters = options.Converters;
                bool hasDateTimeConverter = false;
                foreach (var item in converters)
                {
                    if (item.GetType().Name.Contains("datetime", StringComparison.OrdinalIgnoreCase))
                    {
                        hasDateTimeConverter = true;
                        break;
                    }
                }
                if (!hasDateTimeConverter)
                {
                    options.Converters.Add(new DateTimeConverter());
                    options.Converters.Add(new DateTimeNullableConverter());
                }
            }
            //忽略大小写
            options.PropertyNameCaseInsensitive = true;
            //允许注释
            options.ReadCommentHandling = JsonCommentHandling.Skip;
            //允许尾随逗号
            options.AllowTrailingCommas = true;
            //允许将字符串读取为数字
            options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            //包含公共字段
            options.IncludeFields = true;
            return options;
        }

        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeToString(object o)
        {
            return SerializeToString(o, null);
        }

        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <param name="options">序列化选项</param>
        /// <returns>json字符串</returns>
        public static string SerializeToString(object o, JsonSerializerOptions options)
        {
            string json = JsonSerializer.Serialize(o, BuildJsonSerializerOptions(options));
            return json;
        }

        /// <summary>
        /// 将对象序列化为JSON格式（便于查看的）
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeToFormatString(object o)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            return SerializeToString(o, options);
        }

        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static byte[] SerializeToByte(object o)
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(o, BuildJsonSerializerOptions(null));
            return json;
        }

        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <param name="options">序列化选项</param>
        /// <returns>json字符串</returns>
        public static byte[] SerializeToByte(object o, JsonSerializerOptions options)
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(o, BuildJsonSerializerOptions(options));
            return json;
        }

        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeStringToObject<T>(string json) where T : class
        {
            return JsonSerializer.Deserialize<T>(json, BuildJsonSerializerOptions(null));
        }


        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static object DeserializeStringToObject(string json, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
        {
            return JsonSerializer.Deserialize(json, type, BuildJsonSerializerOptions(null));
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeStringToList<T>(string json) where T : class
        {
            return JsonSerializer.Deserialize<List<T>>(json, BuildJsonSerializerOptions(null));
        }

        /// <summary>
        /// 解析JSON byte数组生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeByteToObject<T>(byte[] json) where T : class
        {
            return JsonSerializer.Deserialize<T>(json, BuildJsonSerializerOptions(null));
        }

        /// <summary>
        /// 解析JSON byte数组生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static object DeserializeByteToObject(byte[] json, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
        {
            return JsonSerializer.Deserialize(json, type, BuildJsonSerializerOptions(null));
        }

        /// <summary>
        /// 解析JSON byte数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeByteToList<T>(byte[] json) where T : class
        {
            return JsonSerializer.Deserialize<List<T>>(json, BuildJsonSerializerOptions(null));
        }
    }
}
