using System;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// JSON 反序列化时兼容 long 数字与字符串两种格式。
    /// </summary>
    public sealed class FlexibleLongConverter : JsonConverter<long?>
    {
        public override long? ReadJson(JsonReader reader, Type objectType, long? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                return Convert.ToInt64(reader.Value);
            }

            if (reader.TokenType == JsonToken.String)
            {
                var s = reader.Value?.ToString();
                if (string.IsNullOrWhiteSpace(s)) return null;
                return long.TryParse(s, out var v) ? v : null;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, long? value, JsonSerializer serializer)
        {
            if (value.HasValue)
            {
                writer.WriteValue(value.Value);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
