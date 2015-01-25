using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    /// <summary>
    /// Used to fix issue with Json.Net from converting string arrays to JArray objects
    /// </summary>
    public class NestedArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<string>) || objectType == typeof(System.Object));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Read();
                var value = new List<string>();

                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.String)
                        value.Add(reader.Value as string);
                    else
                        throw new NotImplementedException(string.Format("Unhandled Json Type '{0}'", reader.TokenType));

                    reader.Read();
                }

                return value.ToArray();
            }

            return serializer.Deserialize(reader);
        }
    }
}
