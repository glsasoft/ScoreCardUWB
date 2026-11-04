using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public class Vector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3) || objectType == typeof(Vector3?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0, z = 0;

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
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            return new Vector3(x, y, z);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Vector3 vector3)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(Math.Round(vector3.x, 3));
                writer.WritePropertyName("y");
                writer.WriteValue(Math.Round(vector3.y, 3));
                writer.WritePropertyName("z");
                writer.WriteValue(Math.Round(vector3.z, 3));
                writer.WriteEndObject();
            }
        }
    }
}
