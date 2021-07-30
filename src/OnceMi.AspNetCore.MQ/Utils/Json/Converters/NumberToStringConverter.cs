using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnceMi.AspNetCore.MQ.Utils
{
    /// <summary>
    /// 数字序列化为字符串
    /// 使用方式：
    /// 1、熟悉上面添加：[JsonConverter(typeof(NumberToStringConverter))]
    /// 2、var deserializeOptions = new JsonSerializerOptions(); deserializeOptions.Converters.Add(new NumberToStringConverter());
    /// 解决问题：
    /// 解决雪花算法生成的id过长导致js丢失精度的问题。
    /// 适用范围：
    /// System.Text.Json
    /// </summary>
    class NumberToStringConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(string) == typeToConvert;
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.TryGetInt64(out long l) ?
                    l.ToString() :
                    reader.GetDouble().ToString();
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                return document.RootElement.Clone().ToString();
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
