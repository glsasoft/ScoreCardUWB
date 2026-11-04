using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public class QuaternionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quaternion)|| objectType == typeof(Quaternion?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0, z = 0, w = 1;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propName = (string)reader.Value;
                    reader.Read();
                    switch (propName)
                    {
                        case "x": x = Convert.ToSingle(reader.Value); break;
                        case "y": y = Convert.ToSingle(reader.Value); break;
                        case "z": z = Convert.ToSingle(reader.Value); break;
                        case "w": w = Convert.ToSingle(reader.Value); break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            return new Quaternion(x, y, z, w);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Quaternion quaternion)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(Math.Round(quaternion.x, 3));
                writer.WritePropertyName("y");
                writer.WriteValue(Math.Round(quaternion.y, 3));
                writer.WritePropertyName("z");
                writer.WriteValue(Math.Round(quaternion.z, 3));
                writer.WritePropertyName("w");
                writer.WriteValue(Math.Round(quaternion.w, 3));
                writer.WriteEndObject();
            }
        }
    }
}
