using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Util.Json
{
    public class ExceptionConverter : JsonConverter<Exception>
    {
        public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(value.Message), value.Message);
            writer.WriteString(nameof(value.StackTrace), value.StackTrace);
            writer.WriteEndObject();
        }
    }
}
